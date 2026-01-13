using OpenMind.Shared.IntegrationMessages;

namespace OpenMind.Fulfillment.IntegrationEvents.Commands;

/// <summary>
/// Command to fulfill an order (async call - shipping).
/// </summary>
public record FulfillOrderCommand : IntegrationCommand
{
    public Guid OrderId { get; init; }
    public Guid CustomerId { get; init; }
    public List<FulfillmentItemDto> Items { get; init; } = [];
    public string ShippingAddress { get; init; } = string.Empty;
}
