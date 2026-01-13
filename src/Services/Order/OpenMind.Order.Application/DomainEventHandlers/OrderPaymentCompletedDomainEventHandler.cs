using MassTransit;
using OpenMind.Order.IntegrationEvents.Events;
using OpenMind.Order.Domain.Events;
using OpenMind.Shared.Application.DomainEvents;

namespace OpenMind.Order.Application.DomainEventHandlers;

public class OrderPaymentCompletedDomainEventHandler(IPublishEndpoint publishEndpoint)
    : IDomainEventHandler<OrderPaymentCompletedDomainEvent>
{
    public async Task Handle(DomainEventNotification<OrderPaymentCompletedDomainEvent> notification, CancellationToken cancellationToken)
    {
        var domainEvent = notification.DomainEvent;

        await publishEndpoint.Publish(new OrderPaymentCompletedEvent
        {
            CorrelationId = domainEvent.CorrelationId,
            OrderId = domainEvent.OrderId,
            TransactionId = domainEvent.TransactionId
        }, cancellationToken);
    }
}
