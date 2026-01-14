using OpenMind.Shared.IntegrationMessages;

namespace OpenMind.Order.Contract.Commands;

/// <summary>
/// Command to mark order payment as failed.
/// </summary>
public record MarkOrderAsPaymentFailedCommand : IntegrationCommand, IOrderCommand
{
    public Guid OrderId { get; init; }
    public string Reason { get; init; } = string.Empty;
}
