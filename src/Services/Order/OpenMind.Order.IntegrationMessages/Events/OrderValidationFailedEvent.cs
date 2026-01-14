using OpenMind.Shared.IntegrationMessages;

namespace OpenMind.Order.IntegrationEvents.Events;

/// <summary>
/// Event indicating order validation failed.
/// </summary>
public record OrderValidationFailedEvent : IntegrationEvent, IOrderEvent
{
    public Guid OrderId { get; init; }
    public string Reason { get; init; } = string.Empty;
}
