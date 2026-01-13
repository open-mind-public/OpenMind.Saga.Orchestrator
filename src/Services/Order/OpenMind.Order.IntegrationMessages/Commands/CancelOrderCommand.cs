using OpenMind.Shared.IntegrationMessages;

namespace OpenMind.Order.IntegrationEvents.Commands;

/// <summary>
/// Command to cancel an order.
/// </summary>
public record CancelOrderCommand : IntegrationCommand
{
    public Guid OrderId { get; init; }
    public string Reason { get; init; } = string.Empty;
}
