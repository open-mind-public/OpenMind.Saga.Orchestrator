namespace OpenMind.Fulfillment.Contract;

public record OutOfStockItemDto
{
    public Guid ProductId { get; init; }
    public string ProductName { get; init; } = string.Empty;
    public int RequestedQuantity { get; init; }
    public int AvailableQuantity { get; init; }
}
