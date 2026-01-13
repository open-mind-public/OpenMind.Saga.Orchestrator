using OpenMind.Shared.IntegrationMessages;

namespace OpenMind.Payment.IntegrationEvents.Commands;

/// <summary>
/// Command to refund payment.
/// </summary>
public record RefundPaymentCommand : IntegrationCommand
{
    public Guid OrderId { get; init; }
    public Guid PaymentId { get; init; }
    public decimal Amount { get; init; }
    public string Reason { get; init; } = string.Empty;
}
