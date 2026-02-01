using Analytics.Processor.Data;
using Analytics.Processor.Models;
using Analytics.Processor.Services;
using Contracts;
using MassTransit;

namespace Analytics.Processor.Consumers;

public class OrderCreatedConsumer : IConsumer<OrderCreated>
{
    private readonly AnalyticsDbContext _context;
    private readonly IKafkaProducerService _kafkaProducer;
    private readonly ILogger<OrderCreatedConsumer> _logger;

    public OrderCreatedConsumer(
        AnalyticsDbContext context,
        IKafkaProducerService kafkaProducer,
        ILogger<OrderCreatedConsumer> logger)
    {
        _context = context;
        _kafkaProducer = kafkaProducer;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<OrderCreated> context)
    {
        var orderCreated = context.Message;
        
        _logger.LogInformation("Processing OrderCreated event for Order {OrderId}", orderCreated.OrderId);

        // Create analytics record
        var analyticsRecord = new AnalyticsRecord
        {
            Id = Guid.NewGuid(),
            OrderId = orderCreated.OrderId,
            CustomerName = orderCreated.CustomerName,
            TotalAmount = orderCreated.TotalAmount,
            ProcessedAt = DateTime.UtcNow,
            ProcessingStatus = "Processed"
        };

        // Save to PostgreSQL
        _context.AnalyticsRecords.Add(analyticsRecord);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Saved analytics record {AnalyticsId} to PostgreSQL", analyticsRecord.Id);

        // Publish to Kafka
        var streamAnalytics = new StreamAnalytics
        {
            AnalyticsId = analyticsRecord.Id,
            OrderId = analyticsRecord.OrderId,
            CustomerName = analyticsRecord.CustomerName,
            TotalAmount = analyticsRecord.TotalAmount,
            ProcessedAt = analyticsRecord.ProcessedAt,
            ProcessingStatus = analyticsRecord.ProcessingStatus
        };

        await _kafkaProducer.PublishStreamAnalyticsAsync(streamAnalytics);

        _logger.LogInformation("Published StreamAnalytics message to Kafka for Analytics {AnalyticsId}", analyticsRecord.Id);
    }
}
