using OpenMind.Shared.IntegrationMessages;

namespace OpenMind.Order.Contract.Commands;

public record ValidateOrderCommand : IntegrationCommand, IOrderCommand
{
    public Guid OrderId { get; init; }
}
