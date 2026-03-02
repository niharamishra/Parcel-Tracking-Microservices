using Confluent.Kafka;
using Microsoft.EntityFrameworkCore;
using Shared.Contracts;
using System.Text.Json;
using Tracking.Domain;
using Tracking.Domain.Enum;
using Tracking.Processor.Data;
using Tracking.Processor.Entities;
using Tracking.Processor.Validators;

public class KafkaConsumerService : BackgroundService
{
    private readonly IConfiguration _configuration;
    private readonly IServiceScopeFactory _scopeFactory;
    private IConsumer<string, string> _consumer;
    private IProducer<string, string> _dlqProducer;

    // In-memory cache for deduplication
    private readonly HashSet<Guid> _processedEvents = new();
    private readonly object _cacheLock = new();
    private readonly int _cacheLimit = 100_000;

    private string _dlqTopic;

    public KafkaConsumerService(IConfiguration configuration, IServiceScopeFactory scopeFactory)
    {
        _configuration = configuration;
        _scopeFactory = scopeFactory;

        var bootstrapServers = _configuration["Kafka:BootstrapServers"];
        var groupId = _configuration["Kafka:GroupId"];
        var topic = _configuration["Kafka:Topic"];
        _dlqTopic = _configuration["Kafka:DLQTopic"] ?? "parcel-events-dlq";

        // Kafka Consumer
        var consumerConfig = new ConsumerConfig
        {
            BootstrapServers = bootstrapServers,
            GroupId = groupId,
            AutoOffsetReset = AutoOffsetReset.Earliest,
            EnableAutoCommit = false
        };
        _consumer = new ConsumerBuilder<string, string>(consumerConfig).Build();
        _consumer.Subscribe(topic);

        // Kafka Producer for DLQ
        var producerConfig = new ProducerConfig { BootstrapServers = bootstrapServers };
        _dlqProducer = new ProducerBuilder<string, string>(producerConfig).Build();

        Console.WriteLine("Kafka Consumer started...");
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await Task.Run(async () =>
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    var result = _consumer.Consume(stoppingToken);
                    await ProcessMessageAsync(result.Message.Value, stoppingToken);

                    // Commit offsets after successful processing
                    _consumer.Commit(result);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Kafka Consume Error: {ex.Message}");
                }
            }
        }, stoppingToken);
    }

    private bool IsDuplicate(Guid eventId)
    {
        lock (_cacheLock)
        {
            if (_processedEvents.Contains(eventId))
                return true;

            _processedEvents.Add(eventId);

            if (_processedEvents.Count > _cacheLimit)
            {
                _processedEvents.Clear();
            }

            return false;
        }
    }

    private async Task<bool> ProcessMessageAsync(string message, CancellationToken cancellationToken)
    {
        using var scope = _scopeFactory.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ParcelDbContext>();

        ParcelEvent parcelEvent = DeserializeOrSendToDlq(message);
        if (parcelEvent == null) return false;

        if (IsDuplicate(parcelEvent.EventId))
            return true;

        using var tx = await db.Database.BeginTransactionAsync(cancellationToken);

        try
        {
            // 1️⃣ Load or create parcel (pricing + header done here)
            var parcel = await db.Parcels
                .FirstOrDefaultAsync(p => p.ParcelId == parcelEvent.ParcelId, cancellationToken);

            if (parcel == null)
            {
                parcel = CreateNewParcel(parcelEvent);
                await db.Parcels.AddAsync(parcel, cancellationToken);
            }

            // 2️⃣ ⬅️ STAGE VALIDATION GOES HERE ⬅️
            var registry = scope.ServiceProvider
                .GetRequiredService<ScanStageValidatorRegistry>();

            var context = new ParcelContext
            {
                Parcel = parcel,
                Event = parcelEvent
            };

            var validation = registry.Validate(parcelEvent.EventType, context);

            if (!validation.IsValid)
            {
                await _dlqProducer.ProduceAsync(
                    _dlqTopic,
                    new Message<string, string>
                    {
                        Key = parcelEvent.EventId.ToString(),
                        Value = $"{validation.FailureReason} | {message}"
                    },
                    cancellationToken
                );

                await tx.RollbackAsync(cancellationToken);
                return false; // ❌ DO NOT COMMIT OFFSET
            }

            // 3️⃣ Apply state change (now safe)
            parcel.CurrentState = parcelEvent.EventType;
            parcel.LastUpdated = DateTime.UtcNow;
            parcel.Version++;

            // 4️⃣ Persist event
            await db.ParcelEvents.AddAsync(new ParcelEventEntity
            {
                EventId = parcelEvent.EventId,
                ParcelId = parcelEvent.ParcelId,
                EventType = parcelEvent.EventType,
                ProcessedAt = DateTime.UtcNow
            }, cancellationToken);

            await db.SaveChangesAsync(cancellationToken);
            await tx.CommitAsync(cancellationToken);

            return true;
        }
        catch (Exception ex)
        {
            await tx.RollbackAsync(cancellationToken);

            await _dlqProducer.ProduceAsync(
                _dlqTopic,
                new Message<string, string>
                {
                    Key = parcelEvent.EventId.ToString(),
                    Value = $"Processing error: {ex.Message} | {message}"
                },
                cancellationToken
            );

            return false;
        }
    }

    private bool IsValidTransition(string current, string next)
    {
        var rules = new Dictionary<string, List<string>>
        {
            { "CREATED", new() { "PICKED_UP" } },
            { "PICKED_UP", new() { "IN_TRANSIT" } },
            { "IN_TRANSIT", new() { "OUT_FOR_DELIVERY" } },
            { "OUT_FOR_DELIVERY", new() { "DELIVERED", "FAILED" } }
        };

        if (current == null || current == next) return true;
        return rules.ContainsKey(current) && rules[current].Contains(next);
    }

    public override void Dispose()
    {
        _consumer.Close();
        _consumer.Dispose();
        _dlqProducer.Dispose();
        base.Dispose();
    }
    private ParcelEvent? DeserializeOrSendToDlq(string message)
    {
        try
        {
            return JsonSerializer.Deserialize<ParcelEvent>(message);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Deserialization failed: {ex.Message}");

            _dlqProducer.Produce(
                _dlqTopic,
                new Message<string, string>
                {
                    Key = Guid.NewGuid().ToString(),
                    Value = $"Deserialization failed | {ex.Message} | {message}"
                }
            );

            return null;
        }
    }
    private Parcels CreateNewParcel(ParcelEvent parcelEvent)
    {
        // 1️⃣ Size + pricing calculation
        var pricing = ParcelSizeClassifier.ClassifyAndPrice(
            parcelEvent.Length,
            parcelEvent.Width,
            parcelEvent.Height,
            parcelEvent.BasePrice
        );

        // 2️⃣ Create parcel aggregate root
        return new Parcels
        {
            ParcelId = parcelEvent.ParcelId,
            CurrentState = parcelEvent.EventType,
            Version = 1,
            LastUpdated = DateTime.UtcNow,

            // Header info
            FromLocation = parcelEvent.FromLocation,
            ToLocation = parcelEvent.ToLocation,

            // Size + pricing
            SizeCategory = pricing.Size.ToString(),
            BaseCharge = pricing.BaseCharge,
            Surcharge = pricing.SizeSurcharge,
            TotalCharge = pricing.TotalCharge
        };
    }
}