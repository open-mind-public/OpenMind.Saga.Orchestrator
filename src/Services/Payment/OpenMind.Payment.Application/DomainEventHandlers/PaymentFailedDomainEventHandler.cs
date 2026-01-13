using MassTransit;
using OpenMind.BuildingBlocks.Application.DomainEvents;
using OpenMind.BuildingBlocks.IntegrationEvents.Payments;
using OpenMind.Payment.Domain.Events;

namespace OpenMind.Payment.Application.DomainEventHandlers;

/// <summary>
/// Handles the PaymentFailedDomainEvent.
/// Publishes the PaymentFailedEvent integration event to notify other services.
/// </summary>
public class PaymentFailedDomainEventHandler(IPublishEndpoint publishEndpoint)
    : IDomainEventHandler<PaymentFailedDomainEvent>
{
    public async Task Handle(DomainEventNotification<PaymentFailedDomainEvent> notification, CancellationToken cancellationToken)
    {
        var domainEvent = notification.DomainEvent;

        // Publish integration event to notify other services (Orchestrator, etc.)
        await publishEndpoint.Publish(new PaymentFailedEvent
        {
            OrderId = domainEvent.OrderId,
            Reason = domainEvent.Reason,
            ErrorCode = "PAYMENT_DECLINED"
        }, cancellationToken);
    }
}
