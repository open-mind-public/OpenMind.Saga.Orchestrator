using OpenMind.Shared.IntegrationMessages;

namespace OpenMind.Order.Contract.Events;

/// <summary>
/// Event indicating order was marked as back ordered.
/// </summary>
public record OrderBackOrderedEvent : IntegrationEvent, IOrderEvent
{
    public Guid OrderId { get; init; }
    public string Reason { get; init; } = string.Empty;
}
