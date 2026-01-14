using OpenMind.Shared.IntegrationMessages;

namespace OpenMind.Order.IntegrationEvents.Events;

/// <summary>
/// Event indicating order was cancelled.
/// </summary>
public record OrderCancelledEvent : IntegrationEvent, IOrderEvent
{
    public Guid OrderId { get; init; }
}
