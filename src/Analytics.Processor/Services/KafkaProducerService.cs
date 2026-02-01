using Confluent.Kafka;
using Contracts;
using System.Text.Json;

namespace Analytics.Processor.Services;

public interface IKafkaProducerService
{
    Task PublishStreamAnalyticsAsync(StreamAnalytics message);
}

public class KafkaProducerService : IKafkaProducerService, IDisposable
{
    private readonly IProducer<string, string> _producer;
    private readonly ILogger<KafkaProducerService> _logger;
    private readonly string _topicName;

    public KafkaProducerService(IConfiguration configuration, ILogger<KafkaProducerService> logger)
    {
        _logger = logger;
        _topicName = configuration["Kafka:Topic"] ?? "stream-analytics";

        var config = new ProducerConfig
        {
            BootstrapServers = configuration["Kafka:BootstrapServers"] ?? "localhost:9092"
        };

        _producer = new ProducerBuilder<string, string>(config).Build();
    }

    public async Task PublishStreamAnalyticsAsync(StreamAnalytics message)
    {
        try
        {
            var messageJson = JsonSerializer.Serialize(message);
            var kafkaMessage = new Message<string, string>
            {
                Key = message.AnalyticsId.ToString(),
                Value = messageJson
            };

            var result = await _producer.ProduceAsync(_topicName, kafkaMessage);
            _logger.LogInformation("Published message to Kafka topic {Topic}, Partition: {Partition}, Offset: {Offset}",
                _topicName, result.Partition.Value, result.Offset.Value);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error publishing message to Kafka");
            throw;
        }
    }

    public void Dispose()
    {
        _producer?.Flush(TimeSpan.FromSeconds(10));
        _producer?.Dispose();
    }
}
