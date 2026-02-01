namespace Contracts;

public record OrderCreated
{
    public Guid OrderId { get; init; }
    public string CustomerName { get; init; } = string.Empty;
    public decimal TotalAmount { get; init; }
    public DateTime CreatedAt { get; init; }
}
