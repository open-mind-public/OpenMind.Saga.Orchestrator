using OpenMind.Shared.IntegrationMessages;

namespace OpenMind.Payment.IntegrationEvents.Events;

/// <summary>
/// Event indicating payment was refunded.
/// </summary>
public record PaymentRefundedEvent : IntegrationEvent
{
    public Guid OrderId { get; init; }
    public Guid PaymentId { get; init; }
    public decimal Amount { get; init; }
}
