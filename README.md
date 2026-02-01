# HybridMessagingDemo

A .NET 10 demonstration application showcasing hybrid messaging and database architecture with two microservices.

## Architecture Overview

This solution demonstrates:
- **Orders.Api**: ASP.NET Core Web API that stores orders in SQL Server and publishes events to RabbitMQ
- **Analytics.Processor**: Background worker service that consumes RabbitMQ events, saves to PostgreSQL, and publishes to Kafka

### Technology Stack

- **.NET 10**: Latest .NET framework
- **SQL Server**: Orders database (Orders.Api)
- **PostgreSQL**: Analytics database (Analytics.Processor)
- **RabbitMQ**: Message broker for event-driven communication (via MassTransit)
- **Apache Kafka**: Streaming platform for analytics events (via Confluent.Kafka)
- **Entity Framework Core**: ORM for both databases
- **MassTransit**: Messaging abstraction library

## Projects

### Orders.Api
- **Database**: SQL Server with EF Core
- **Messaging**: Publishes `OrderCreated` events to RabbitMQ using MassTransit
- **Endpoints**:
  - `POST /api/orders` - Create a new order
  - `GET /api/orders/{id}` - Get order by ID

### Analytics.Processor
- **Consumer**: Consumes `OrderCreated` events from RabbitMQ
- **Database**: PostgreSQL with EF Core for storing processed analytics
- **Producer**: Publishes `StreamAnalytics` messages to Kafka using Confluent.Kafka

### Contracts
- Shared message contracts/events between services
- `OrderCreated` - Event published when an order is created
- `StreamAnalytics` - Message published to Kafka stream

## Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- [Docker Desktop](https://www.docker.com/products/docker-desktop) (for running infrastructure services)

## Getting Started

### 1. Start Infrastructure Services

Start all required infrastructure services using Docker Compose:

```bash
docker-compose up -d
```

This will start:
- SQL Server (port 1433)
- PostgreSQL (port 5432)
- RabbitMQ (ports 5672, 15672 for management UI)
- Kafka (port 9092)
- Zookeeper (port 2181)

You can access the RabbitMQ Management UI at http://localhost:15672 (username: guest, password: guest)

### 2. Apply Database Migrations

Create and apply migrations for both databases:

**For Orders.Api (SQL Server):**
```bash
cd src/Orders.Api
dotnet ef migrations add InitialCreate
dotnet ef database update
```

**For Analytics.Processor (PostgreSQL):**
```bash
cd src/Analytics.Processor
dotnet ef migrations add InitialCreate
dotnet ef database update
```

### 3. Run the Applications

Open two terminal windows:

**Terminal 1 - Run Orders.Api:**
```bash
cd src/Orders.Api
dotnet run
```

**Terminal 2 - Run Analytics.Processor:**
```bash
cd src/Analytics.Processor
dotnet run
```

### 4. Test the Flow

Create an order using curl or your favorite API client:

```bash
curl -X POST http://localhost:5097/api/orders \
  -H "Content-Type: application/json" \
  -d '{
    "customerName": "John Doe",
    "totalAmount": 99.99
  }'
```

This will:
1. Save the order to SQL Server
2. Publish an `OrderCreated` event to RabbitMQ
3. Analytics.Processor consumes the event
4. Save analytics record to PostgreSQL
5. Publish `StreamAnalytics` message to Kafka

## Project Structure

```
HybridMessagingDemo/
├── src/
│   ├── Orders.Api/              # Web API for order management
│   │   ├── Controllers/         # API controllers
│   │   ├── Data/                # EF Core DbContext
│   │   ├── Models/              # Domain models
│   │   └── Program.cs           # App configuration
│   ├── Analytics.Processor/     # Background worker service
│   │   ├── Consumers/           # MassTransit consumers
│   │   ├── Data/                # EF Core DbContext
│   │   ├── Models/              # Domain models
│   │   ├── Services/            # Kafka producer service
│   │   └── Program.cs           # App configuration
│   └── Contracts/               # Shared message contracts
│       ├── OrderCreated.cs      # Order event contract
│       └── StreamAnalytics.cs   # Analytics message contract
├── docker-compose.yml           # Infrastructure services
└── HybridMessagingDemo.sln      # Solution file
```

## Configuration

> **Note:** The configuration files contain hardcoded credentials for demonstration purposes only. In production environments, use:
> - User Secrets for local development
> - Environment variables for containerized deployments
> - Azure Key Vault, AWS Secrets Manager, or similar for production secrets

### Orders.Api (appsettings.json)
```json
{
  "ConnectionStrings": {
    "OrdersDb": "Server=localhost,1433;Database=OrdersDb;User Id=sa;Password=YourStrong@Passw0rd;TrustServerCertificate=True;"
  },
  "RabbitMQ": {
    "Host": "localhost",
    "Username": "guest",
    "Password": "guest"
  }
}
```

### Analytics.Processor (appsettings.json)
```json
{
  "ConnectionStrings": {
    "AnalyticsDb": "Host=localhost;Port=5432;Database=AnalyticsDb;Username=postgres;Password=postgres"
  },
  "RabbitMQ": {
    "Host": "localhost",
    "Username": "guest",
    "Password": "guest"
  },
  "Kafka": {
    "BootstrapServers": "localhost:9092",
    "Topic": "stream-analytics"
  }
}
```

## Cleanup

To stop and remove all infrastructure services:

```bash
docker-compose down -v
```

The `-v` flag removes the volumes, completely cleaning up all data.

## License

This is a demonstration project for educational purposes.