using OpenMind.Shared.IntegrationMessages;

namespace OpenMind.Order.IntegrationEvents.Commands;

/// <summary>
/// Command to mark order payment as failed.
/// </summary>
public record MarkOrderAsPaymentFailedCommand : IntegrationCommand
{
    public Guid OrderId { get; init; }
    public string Reason { get; init; } = string.Empty;
}
