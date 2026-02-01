namespace Analytics.Processor.Models;

public class AnalyticsRecord
{
    public Guid Id { get; set; }
    public Guid OrderId { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public decimal TotalAmount { get; set; }
    public DateTime ProcessedAt { get; set; }
    public string ProcessingStatus { get; set; } = string.Empty;
}
