namespace OpenMind.Fulfillment.Contract;

public record FulfillmentItemDto
{
    public Guid ProductId { get; init; }
    public string ProductName { get; init; } = string.Empty;
    public int Quantity { get; init; }
}
