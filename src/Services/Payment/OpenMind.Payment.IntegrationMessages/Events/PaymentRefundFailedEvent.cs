using OpenMind.Shared.IntegrationMessages;

namespace OpenMind.Payment.IntegrationEvents.Events;

/// <summary>
/// Event indicating payment refund failed.
/// </summary>
public record PaymentRefundFailedEvent : IntegrationEvent
{
    public Guid OrderId { get; init; }
    public string Reason { get; init; } = string.Empty;
}
