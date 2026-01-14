using OpenMind.Shared.IntegrationMessages;

namespace OpenMind.Fulfillment.IntegrationEvents.Events;

/// <summary>
/// Event indicating order was shipped.
/// </summary>
public record OrderShippedEvent : IntegrationEvent, IFulfillmentEvent
{
    public Guid OrderId { get; init; }
    public Guid FulfillmentId { get; init; }
    public string TrackingNumber { get; init; } = string.Empty;
    public DateTime EstimatedDelivery { get; init; }
}
