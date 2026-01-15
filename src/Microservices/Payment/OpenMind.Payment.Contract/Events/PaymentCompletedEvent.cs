using OpenMind.Shared.IntegrationMessages;

namespace OpenMind.Payment.Contract.Events;

/// <summary>
/// Event indicating payment was successfully processed.
/// </summary>
public record PaymentCompletedEvent : IntegrationEvent, IPaymentEvent
{
    public Guid OrderId { get; init; }
    public Guid PaymentId { get; init; }
    public decimal Amount { get; init; }
    public string TransactionId { get; init; } = string.Empty;
}
