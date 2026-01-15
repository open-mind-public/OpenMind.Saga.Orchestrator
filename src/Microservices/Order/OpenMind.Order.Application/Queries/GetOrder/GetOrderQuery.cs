using OpenMind.Shared.Application.Queries;

namespace OpenMind.Order.Application.Queries.GetOrder;

public record GetOrderQuery(Guid OrderId) : IQuery<OrderDto>;

public record OrderDto
{
    public Guid Id { get; init; }
    public Guid CustomerId { get; init; }
    public string Status { get; init; } = string.Empty;
    public decimal TotalAmount { get; init; }
    public string ShippingAddress { get; init; } = string.Empty;
    public string? TrackingNumber { get; init; }
    public List<OrderItemDto> Items { get; init; } = [];
    public DateTime CreatedAt { get; init; }
}

public record OrderItemDto
{
    public Guid ProductId { get; init; }
    public string ProductName { get; init; } = string.Empty;
    public int Quantity { get; init; }
    public decimal UnitPrice { get; init; }
    public decimal TotalPrice { get; init; }
}
