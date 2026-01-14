using OpenMind.Shared.IntegrationMessages;

namespace OpenMind.Order.IntegrationEvents.Events;

/// <summary>
/// Event indicating order payment was marked as completed.
/// </summary>
public record OrderPaymentCompletedEvent : IntegrationEvent, IOrderEvent
{
    public Guid OrderId { get; init; }
    public string TransactionId { get; init; } = string.Empty;
}
