using OpenMind.Shared.IntegrationMessages;

namespace OpenMind.Fulfillment.Contract.Events;

/// <summary>
/// Event indicating item is on backorder.
/// </summary>
public record ItemBackorderedEvent : IntegrationEvent, IFulfillmentEvent
{
    public Guid OrderId { get; init; }
    public Guid FulfillmentId { get; init; }
    public List<BackorderItemDto> BackorderedItems { get; init; } = [];
    public DateTime EstimatedAvailability { get; init; }
}
