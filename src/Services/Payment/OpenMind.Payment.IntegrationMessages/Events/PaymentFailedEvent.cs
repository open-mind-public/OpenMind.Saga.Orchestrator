using OpenMind.Shared.IntegrationMessages;

namespace OpenMind.Payment.IntegrationEvents.Events;

/// <summary>
/// Event indicating payment processing failed.
/// </summary>
public record PaymentFailedEvent : IntegrationEvent, IPaymentEvent
{
    public Guid OrderId { get; init; }
    public string Reason { get; init; } = string.Empty;
    public string ErrorCode { get; init; } = string.Empty;
}
