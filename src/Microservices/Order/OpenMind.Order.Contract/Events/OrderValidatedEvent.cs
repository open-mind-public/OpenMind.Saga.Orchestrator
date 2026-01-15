using OpenMind.Shared.IntegrationMessages;

namespace OpenMind.Order.Contract.Events;

/// <summary>
/// Event indicating order was validated and ready for placement.
/// Contains order details retrieved from the Order Service.
/// </summary>
public record OrderValidatedEvent : IntegrationEvent, IOrderEvent
{
    public Guid OrderId { get; init; }
    public Guid CustomerId { get; init; }
    public decimal TotalAmount { get; init; }
    public string ShippingAddress { get; init; } = string.Empty;
    public string CustomerEmail { get; init; } = string.Empty;
    public string CustomerName { get; init; } = string.Empty;
    public List<OrderItemDto> Items { get; init; } = [];
}
