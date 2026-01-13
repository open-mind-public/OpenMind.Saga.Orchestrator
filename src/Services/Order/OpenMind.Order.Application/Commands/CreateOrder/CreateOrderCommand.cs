using OpenMind.BuildingBlocks.Application.Commands;

namespace OpenMind.Order.Application.Commands.CreateOrder;

public record CreateOrderCommand : ICommand<Guid>
{
    public Guid OrderId { get; init; }
    public Guid CustomerId { get; init; }
    public List<OrderItemCommand> Items { get; init; } = [];
    public string Street { get; init; } = string.Empty;
    public string City { get; init; } = string.Empty;
    public string State { get; init; } = string.Empty;
    public string ZipCode { get; init; } = string.Empty;
    public string Country { get; init; } = string.Empty;
}

public record OrderItemCommand
{
    public Guid ProductId { get; init; }
    public string ProductName { get; init; } = string.Empty;
    public int Quantity { get; init; }
    public decimal UnitPrice { get; init; }
}
