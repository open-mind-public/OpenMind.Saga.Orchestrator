using OpenMind.Shared.IntegrationMessages;

namespace OpenMind.Fulfillment.IntegrationEvents.Events;

/// <summary>
/// Event indicating fulfillment was cancelled.
/// </summary>
public record FulfillmentCancelledEvent : IntegrationEvent
{
    public Guid OrderId { get; init; }
    public Guid FulfillmentId { get; init; }
}
