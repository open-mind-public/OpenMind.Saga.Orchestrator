using OpenMind.Shared.IntegrationMessages;

namespace OpenMind.Order.Contract.Commands;

/// <summary>
/// Command to mark order as shipped.
/// </summary>
public record MarkOrderAsShippedCommand : IntegrationCommand, IOrderCommand
{
    public Guid OrderId { get; init; }
    public string TrackingNumber { get; init; } = string.Empty;
}
