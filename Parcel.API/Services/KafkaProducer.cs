using Confluent.Kafka;
using Microsoft.Extensions.Configuration;
using Shared.Contracts;
using System.Text.Json;

namespace Parcel.API.Services
{
    public class KafkaProducer : IDisposable
    {
        private readonly IProducer<string, string> _producer;
        private readonly string _topic;

        public KafkaProducer(IConfiguration configuration)
        {
            var bootstrapServers = configuration["Kafka:BootstrapServers"];
            _topic = configuration["Kafka:Topic"];

            var config = new ProducerConfig
            {
                BootstrapServers = bootstrapServers,
                Acks = Acks.All,                 // Strong durability
                EnableIdempotence = true         // Prevent duplicate writes
            };

            _producer = new ProducerBuilder<string, string>(config)
                .SetErrorHandler((_, e) =>
                {
                    Console.WriteLine($"Kafka Error: {e.Reason}");
                })
                .Build();
        }

        public async Task PublishAsync(ParcelEvent parcelEvent)
        {
            try
            {
                var message = new Message<string, string>
                {
                    Key = parcelEvent.ParcelId, // Ensures ordering per parcel
                    Value = JsonSerializer.Serialize(parcelEvent)
                };

                var deliveryResult = await _producer.ProduceAsync(_topic, message);

                Console.WriteLine($"Message delivered to: {deliveryResult.TopicPartitionOffset}");
            }
            catch (ProduceException<string, string> ex)
            {
                Console.WriteLine($"Delivery failed: {ex.Error.Reason}");
                throw;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Unexpected error: {ex.Message}");
                throw;
            }
        }

        public void Dispose()
        {
            _producer.Flush(TimeSpan.FromSeconds(5));
            _producer.Dispose();
        }
    }
}