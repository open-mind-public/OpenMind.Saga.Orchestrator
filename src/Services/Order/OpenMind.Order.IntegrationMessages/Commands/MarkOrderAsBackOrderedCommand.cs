using OpenMind.Shared.IntegrationMessages;

namespace OpenMind.Order.IntegrationEvents.Commands;

/// <summary>
/// Command to mark order as back ordered.
/// </summary>
public record MarkOrderAsBackOrderedCommand : IntegrationCommand, IOrderCommand
{
    public Guid OrderId { get; init; }
    public string Reason { get; init; } = string.Empty;
}
