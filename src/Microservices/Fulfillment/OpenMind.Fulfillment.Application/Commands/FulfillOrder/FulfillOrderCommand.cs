using OpenMind.Shared.Application.Commands;

namespace OpenMind.Fulfillment.Application.Commands.FulfillOrder;

public record FulfillOrderCommand : ICommand<FulfillOrderResult>
{
    public Guid OrderId { get; init; }
    public Guid CustomerId { get; init; }
    public List<FulfillmentItemCommand> Items { get; init; } = [];
    public string ShippingAddress { get; init; } = string.Empty;
    public Guid CorrelationId { get; init; }
}

public record FulfillmentItemCommand(Guid ProductId, string ProductName, int Quantity);

public record FulfillOrderResult
{
    public Guid FulfillmentId { get; init; }
    public string? TrackingNumber { get; init; }
    public bool IsBackOrdered { get; init; }
    public List<string> BackOrderedItems { get; init; } = [];
}
