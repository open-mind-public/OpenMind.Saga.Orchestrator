using OpenMind.Shared.IntegrationMessages;

namespace OpenMind.Email.IntegrationEvents.Commands;

/// <summary>
/// Command to send order cancellation email.
/// </summary>
public record SendOrderCancelledEmailCommand : IntegrationCommand
{
    public Guid OrderId { get; init; }
    public Guid CustomerId { get; init; }
    public string CustomerEmail { get; init; } = string.Empty;
    public string CustomerName { get; init; } = string.Empty;
    public string CancellationReason { get; init; } = string.Empty;
}
