# Parcel Tracking POC

Short overview
- Purpose: small proof-of-concept showing event-driven parcel tracking using Kafka, an ASP.NET Web API producer and a .NET background consumer that persists a read model.
- Technologies: .NET 8, ASP.NET Core Web API, BackgroundService, Confluent.Kafka, EF Core (SQL Server LocalDB), Swagger/OpenAPI.

Solution structure
- ParcelTrackingPOC (solution root)
  - Parcel.API/ — ASP.NET Core Web API
    - Purpose: publish parcel events to Kafka.
    - Key files:
      - `Program.cs` — startup, DI, Swagger.
      - `Controllers/ParcelController.cs` — POST `/api/parcels/events`.
      - `Services/KafkaProducer.cs` — publishes `Shared.Contracts.ParcelEvent`.
    - Config: `appsettings.json` (Kafka section).
  - Tracking.Processor/ — Web + Worker (consumer)
    - Purpose: Kafka consumer BackgroundService that processes events, updates read model and persists events; exposes tracking read APIs.
    - Key files:
      - `Program.cs` — DI, DbContext, hosted services, Swagger.
      - `KafkaConsumerService.cs` — BackgroundService consumer, DLQ handling, deduplication, transactional saves.
      - `Data/ParcelDbContext.cs` — EF Core DbContext.
      - `Controllers/TrackingController.cs` — GET endpoints for parcel state and events.
      - `Entities/Parcels.cs`, `Entities/ParcelEventEntity.cs`
      - `appsettings.json` — connection string + Kafka settings (Topic, DLQTopic).
  - Shared.Contracts/ — shared DTOs
    - `ParcelEvent.cs` — event contract used by producer and consumer.
  - Tracking.Domain/ — domain logic
    - `ParcelSizeClassifier.cs`, `ParcelPricing.cs`, `Enum/ParcelSize.cs`
  - Parcel.Repositories/ — repository implementation for read model access
    - `ParcelRepository.cs`, repository interfaces.
  - README.md (this file)

Prerequisites
- .NET 8 SDK installed.
- Kafka broker accessible at configured address (default: `localhost:9092`).
- SQL Server LocalDB (or other SQL Server) available per `Tracking.Processor/appsettings.json` `ConnectionStrings:ParcelDb`.
- (Optional) Docker if you prefer running Kafka + Zookeeper via containers.

Quick start — run both services
1. In Visual Studio:
   - Open solution.
   - Use __Set Startup Projects...__ → choose __Multiple startup projects__ and set `Parcel.API` and `Tracking.Processor` to __Start__.
   - Press F5 or __Debug > Start Debugging__.
2. Or using terminals (two separate terminals):
   - Terminal 1:
     - `cd Parcel.API`
     - `dotnet run`
   - Terminal 2:
     - `cd Tracking.Processor`
     - `dotnet run`

Database
- Create/update DB via EF Core migrations (if migrations exist):
  - From `Tracking.Processor` project folder:
    - `dotnet ef database update`
- Inspect DB using SQL Server Object Explorer or tools (the connection string is in `Tracking.Processor/appsettings.json`).

Configuration keys to check (appsettings.json)
- Kafka:
  - `Kafka:BootstrapServers` (e.g. `localhost:9092`)
  - `Kafka:Topic` (e.g. `parcel-tracking-events`)
  - `Kafka:DLQTopic` (e.g. `parcel-tracking-events-dlq`)
  - `Kafka:GroupId`
- Database:
  - `ConnectionStrings:ParcelDb`

API surfaces (Swagger)
- Parcel.API (producer):
  - POST `/api/parcels/events` — publish `ParcelEvent`.
  - Swagger UI: `https://<parcel-api-url>/swagger` (port printed by app on startup).
- Tracking.Processor (read model & admin):
  - GET `/api/tracking/{trackingId}` — current parcel state.
  - GET `/api/tracking/{trackingId}/events` — events for parcel.
  - Swagger UI: `https://<tracking-processor-url>/swagger` (or root if `RoutePrefix` set to empty).

How to test end-to-end
1. Publish a sample event (replace URL/port):
   - curl:
     - `curl -k -X POST "https://localhost:5001/api/parcels/events" -H "Content-Type: application/json" -d '{ "EventId":"d9b2d63d-a233-4123-9f3b-b2a7b2c3b9a1", "ParcelId":"PKG-1001", "EventType":"PICKED_UP", "Location":"Warehouse A", "Timestamp":"2026-03-01T14:30:00Z" }'`
2. Verify:
   - Parcel.API logs: `Message delivered to: <TopicPartitionOffset>`.
   - Tracking.Processor logs: consumer output `Received Event: ...` and `Database updated successfully.`
   - Query read model via GET `/api/tracking/PKG-1001`.
   - Inspect DB tables `Parcels` and `ParcelEvents`.

DLQ and error handling
- Messages failing deserialization, invalid state transitions, or persistent processing errors are published to the configured DLQ topic (`Kafka:DLQTopic`).
- Consumer commits offsets after processing or after moving a message to the DLQ to avoid reprocessing.
- Deduplication: in-memory HashSet prevents reprocessing of recent event IDs (bounded).

Developer notes
- Background consumers use `BackgroundService` implementation.
- Domain constants (parcel dimension threshold) are centralized under `Tracking.Domain.Constants`.
- Keep connected Kafka topic names and DB connection strings synchronized between projects.
- For production: replace in-memory dedup cache with a durable store if needed, secure Kafka/DB connections, tune commit strategy and DLQ retention.

Common commands
- Build solution:
  - `dotnet build`
- Run single project:
  - `dotnet run --project Parcel.API`
  - `dotnet run --project Tracking.Processor`
- EF Core migrations (from `Tracking.Processor`):
  - `dotnet ef migrations add <Name>`
  - `dotnet ef database update`
