using OpenMind.Shared.IntegrationMessages;

namespace OpenMind.Email.Contract.Commands;

/// <summary>
/// Command to send payment failure notification email.
/// </summary>
public record SendPaymentFailedEmailCommand : IntegrationCommand, IEmailCommand
{
    public Guid OrderId { get; init; }
    public Guid CustomerId { get; init; }
    public string CustomerEmail { get; init; } = string.Empty;
    public string CustomerName { get; init; } = string.Empty;
    public string FailureReason { get; init; } = string.Empty;
}
