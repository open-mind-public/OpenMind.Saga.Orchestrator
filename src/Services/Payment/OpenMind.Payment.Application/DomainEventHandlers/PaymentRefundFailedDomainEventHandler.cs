using MassTransit;
using OpenMind.BuildingBlocks.Application.DomainEvents;
using OpenMind.BuildingBlocks.IntegrationEvents.Payments;
using OpenMind.Payment.Domain.Events;

namespace OpenMind.Payment.Application.DomainEventHandlers;

public class PaymentRefundFailedDomainEventHandler(IPublishEndpoint publishEndpoint)
    : IDomainEventHandler<PaymentRefundFailedDomainEvent>
{
    public async Task Handle(DomainEventNotification<PaymentRefundFailedDomainEvent> notification, CancellationToken cancellationToken)
    {
        var domainEvent = notification.DomainEvent;

        await publishEndpoint.Publish(new PaymentRefundFailedEvent
        {
            CorrelationId = domainEvent.CorrelationId,
            OrderId = domainEvent.OrderId,
            Reason = domainEvent.Reason
        }, cancellationToken);
    }
}
