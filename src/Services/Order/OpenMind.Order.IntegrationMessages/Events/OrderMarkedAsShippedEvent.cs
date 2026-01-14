using OpenMind.Shared.IntegrationMessages;

namespace OpenMind.Order.IntegrationEvents.Events;

/// <summary>
/// Event indicating order status was marked as shipped.
/// </summary>
public record OrderMarkedAsShippedEvent : IntegrationEvent, IOrderEvent
{
    public Guid OrderId { get; init; }
    public string TrackingNumber { get; init; } = string.Empty;
}
