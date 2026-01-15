using OpenMind.Shared.IntegrationMessages;

namespace OpenMind.Order.Contract.Commands;

/// <summary>
/// Command to cancel an order.
/// </summary>
public record CancelOrderCommand : IntegrationCommand, IOrderCommand
{
    public Guid OrderId { get; init; }
    public string Reason { get; init; } = string.Empty;
}
