using OpenMind.Shared.IntegrationMessages;

namespace OpenMind.Fulfillment.Contract.Commands;

/// <summary>
/// Command to fulfill an order (async call - shipping).
/// </summary>
public record FulfillOrderCommand : IntegrationCommand, IFulfillmentCommand
{
    public Guid OrderId { get; init; }
    public Guid CustomerId { get; init; }
    public List<FulfillmentItemDto> Items { get; init; } = [];
    public string ShippingAddress { get; init; } = string.Empty;
}
