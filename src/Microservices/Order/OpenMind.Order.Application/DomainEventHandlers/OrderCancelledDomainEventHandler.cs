using MassTransit;
using OpenMind.Order.Contract.Events;
using OpenMind.Order.Domain.Events;
using OpenMind.Shared.Application.DomainEvents;

namespace OpenMind.Order.Application.DomainEventHandlers;

public class OrderCancelledDomainEventHandler(IPublishEndpoint publishEndpoint)
    : IDomainEventHandler<OrderCancelledDomainEvent>
{
    public async Task Handle(DomainEventNotification<OrderCancelledDomainEvent> notification, CancellationToken cancellationToken)
    {
        var domainEvent = notification.DomainEvent;

        await publishEndpoint.Publish(new OrderCancelledEvent
        {
            CorrelationId = domainEvent.CorrelationId,
            OrderId = domainEvent.OrderId
        }, cancellationToken);
    }
}
