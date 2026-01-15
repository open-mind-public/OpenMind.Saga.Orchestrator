using OpenMind.Shared.IntegrationMessages;

namespace OpenMind.Order.Contract.Commands;

public record PlaceOrderCommand : IntegrationCommand, IOrderCommand
{
    public Guid OrderId { get; init; }
}
