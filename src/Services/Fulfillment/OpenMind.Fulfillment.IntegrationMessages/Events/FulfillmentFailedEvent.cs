using OpenMind.Shared.IntegrationMessages;

namespace OpenMind.Fulfillment.IntegrationEvents.Events;

/// <summary>
/// Event indicating fulfillment failed due to stock issues.
/// </summary>
public record FulfillmentFailedEvent : IntegrationEvent, IFulfillmentEvent
{
    public Guid OrderId { get; init; }
    public string Reason { get; init; } = string.Empty;
    public List<OutOfStockItemDto> OutOfStockItems { get; init; } = [];
}
