using OpenMind.Shared.IntegrationMessages;

namespace OpenMind.Order.Contract.Events;

/// <summary>
/// Event indicating order payment was marked as failed.
/// </summary>
public record OrderPaymentFailedEvent : IntegrationEvent, IOrderEvent
{
    public Guid OrderId { get; init; }
    public string Reason { get; init; } = string.Empty;
}
