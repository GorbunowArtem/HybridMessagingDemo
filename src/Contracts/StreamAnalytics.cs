namespace Contracts;

public record StreamAnalytics
{
    public Guid AnalyticsId { get; init; }
    public Guid OrderId { get; init; }
    public string CustomerName { get; init; } = string.Empty;
    public decimal TotalAmount { get; init; }
    public DateTime ProcessedAt { get; init; }
    public string ProcessingStatus { get; init; } = string.Empty;
}
