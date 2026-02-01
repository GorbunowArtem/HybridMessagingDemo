using Contracts;
using MassTransit;
using Microsoft.AspNetCore.Mvc;
using Orders.Api.Data;
using Orders.Api.Models;

namespace Orders.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class OrdersController : ControllerBase
{
    private readonly OrdersDbContext _context;
    private readonly IPublishEndpoint _publishEndpoint;
    private readonly ILogger<OrdersController> _logger;

    public OrdersController(
        OrdersDbContext context,
        IPublishEndpoint publishEndpoint,
        ILogger<OrdersController> logger)
    {
        _context = context;
        _publishEndpoint = publishEndpoint;
        _logger = logger;
    }

    [HttpPost]
    public async Task<ActionResult<Order>> CreateOrder([FromBody] CreateOrderRequest request)
    {
        var order = new Order
        {
            Id = Guid.NewGuid(),
            CustomerName = request.CustomerName,
            TotalAmount = request.TotalAmount,
            CreatedAt = DateTime.UtcNow
        };

        _context.Orders.Add(order);
        await _context.SaveChangesAsync();

        var orderCreatedEvent = new OrderCreated
        {
            OrderId = order.Id,
            CustomerName = order.CustomerName,
            TotalAmount = order.TotalAmount,
            CreatedAt = order.CreatedAt
        };

        await _publishEndpoint.Publish(orderCreatedEvent);

        _logger.LogInformation("Order {OrderId} created and event published", order.Id);

        return CreatedAtAction(nameof(GetOrder), new { id = order.Id }, order);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<Order>> GetOrder(Guid id)
    {
        var order = await _context.Orders.FindAsync(id);
        if (order == null)
        {
            return NotFound();
        }

        return order;
    }
}

public record CreateOrderRequest
{
    public string CustomerName { get; init; } = string.Empty;
    public decimal TotalAmount { get; init; }
}
