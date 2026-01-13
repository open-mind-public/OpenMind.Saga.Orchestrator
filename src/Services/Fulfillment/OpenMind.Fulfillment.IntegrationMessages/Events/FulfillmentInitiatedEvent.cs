using OpenMind.Shared.IntegrationMessages;

namespace OpenMind.Fulfillment.IntegrationEvents.Events;

/// <summary>
/// Event indicating fulfillment was initiated.
/// </summary>
public record FulfillmentInitiatedEvent : IntegrationEvent
{
    public Guid OrderId { get; init; }
    public Guid FulfillmentId { get; init; }
    public string TrackingNumber { get; init; } = string.Empty;
}
