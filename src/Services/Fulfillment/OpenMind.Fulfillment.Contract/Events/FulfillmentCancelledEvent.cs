using OpenMind.Shared.IntegrationMessages;

namespace OpenMind.Fulfillment.Contract.Events;

/// <summary>
/// Event indicating fulfillment was cancelled.
/// </summary>
public record FulfillmentCancelledEvent : IntegrationEvent, IFulfillmentEvent
{
    public Guid OrderId { get; init; }
    public Guid FulfillmentId { get; init; }
}
