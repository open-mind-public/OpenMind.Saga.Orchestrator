using OpenMind.Shared.IntegrationMessages;

namespace OpenMind.Order.IntegrationEvents.Commands;

/// <summary>
/// Command to mark order payment as completed.
/// </summary>
public record MarkOrderAsPaymentCompletedCommand : IntegrationCommand, IOrderCommand
{
    public Guid OrderId { get; init; }
    public string TransactionId { get; init; } = string.Empty;
}
