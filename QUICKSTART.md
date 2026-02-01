# Quick Start Guide

This guide helps you get the HybridMessagingDemo application up and running quickly.

## Prerequisites

- .NET 10 SDK
- Docker Desktop (for infrastructure services)

## Fast Setup (5 minutes)

### 1. Start Infrastructure
```bash
docker-compose up -d
```

Wait ~30 seconds for services to start.

### 2. Create Databases

**Orders.Api (SQL Server):**
```bash
cd src/Orders.Api
dotnet ef migrations add InitialCreate
dotnet ef database update
cd ../..
```

**Analytics.Processor (PostgreSQL):**
```bash
cd src/Analytics.Processor
dotnet ef migrations add InitialCreate
dotnet ef database update
cd ../..
```

### 3. Run the Applications

Open two terminal windows:

**Terminal 1:**
```bash
cd src/Orders.Api
dotnet run
```

**Terminal 2:**
```bash
cd src/Analytics.Processor
dotnet run
```

### 4. Test It!

Create an order:
```bash
curl -X POST http://localhost:5097/api/orders \
  -H "Content-Type: application/json" \
  -d '{"customerName": "Jane Smith", "totalAmount": 149.99}'
```

## What Happens?

1. Order is saved to SQL Server ✅
2. OrderCreated event is published to RabbitMQ ✅
3. Analytics.Processor consumes the event ✅
4. Analytics record is saved to PostgreSQL ✅
5. StreamAnalytics message is published to Kafka ✅

## Check the Logs

You'll see log messages in both terminals showing the flow:
- **Orders.Api**: "Order {OrderId} created and event published"
- **Analytics.Processor**: "Processing OrderCreated event...", "Saved analytics record...", "Published StreamAnalytics message..."

## Management UIs

- **RabbitMQ**: http://localhost:15672 (guest/guest)

## Cleanup

```bash
docker-compose down -v
```

## Troubleshooting

**"Connection refused" errors?**
- Ensure Docker services are running: `docker-compose ps`

**"Database does not exist"?**
- Run the EF migrations (step 2 above)

**Port conflicts?**
- Check if ports 1433, 5432, 5672, 9092, or 5097 are in use
