using OpenMind.Shared.IntegrationMessages;

namespace OpenMind.Payment.Contract.Events;

/// <summary>
/// Event indicating payment was refunded.
/// </summary>
public record PaymentRefundedEvent : IntegrationEvent, IPaymentEvent
{
    public Guid OrderId { get; init; }
    public Guid PaymentId { get; init; }
    public decimal Amount { get; init; }
}
