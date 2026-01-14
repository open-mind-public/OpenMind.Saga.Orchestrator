using OpenMind.Shared.IntegrationMessages;

namespace OpenMind.Order.Contract.Commands;

/// <summary>
/// Command to mark order as back ordered.
/// </summary>
public record MarkOrderAsBackOrderedCommand : IntegrationCommand, IOrderCommand
{
    public Guid OrderId { get; init; }
    public string Reason { get; init; } = string.Empty;
}
