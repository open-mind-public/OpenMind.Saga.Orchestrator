using MassTransit;
using OpenMind.Order.IntegrationEvents.Events;
using OpenMind.Order.Domain.Events;
using OpenMind.Shared.Application.DomainEvents;

namespace OpenMind.Order.Application.DomainEventHandlers;

public class OrderPaymentFailedDomainEventHandler(IPublishEndpoint publishEndpoint)
    : IDomainEventHandler<OrderPaymentFailedDomainEvent>
{
    public async Task Handle(DomainEventNotification<OrderPaymentFailedDomainEvent> notification, CancellationToken cancellationToken)
    {
        var domainEvent = notification.DomainEvent;

        await publishEndpoint.Publish(new OrderPaymentFailedEvent
        {
            CorrelationId = domainEvent.CorrelationId,
            OrderId = domainEvent.OrderId,
            Reason = domainEvent.Reason
        }, cancellationToken);
    }
}
