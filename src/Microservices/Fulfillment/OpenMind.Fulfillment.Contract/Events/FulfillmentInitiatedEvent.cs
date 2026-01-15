using OpenMind.Shared.IntegrationMessages;

namespace OpenMind.Fulfillment.Contract.Events;

/// <summary>
/// Event indicating fulfillment was initiated.
/// </summary>
public record FulfillmentInitiatedEvent : IntegrationEvent, IFulfillmentEvent
{
    public Guid OrderId { get; init; }
    public Guid FulfillmentId { get; init; }
    public string TrackingNumber { get; init; } = string.Empty;
}
