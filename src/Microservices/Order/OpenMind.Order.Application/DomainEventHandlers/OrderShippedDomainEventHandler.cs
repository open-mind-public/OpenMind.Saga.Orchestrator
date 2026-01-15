using MassTransit;
using OpenMind.Order.Contract.Events;
using OpenMind.Order.Domain.Events;
using OpenMind.Shared.Application.DomainEvents;

namespace OpenMind.Order.Application.DomainEventHandlers;

public class OrderShippedDomainEventHandler(IPublishEndpoint publishEndpoint)
    : IDomainEventHandler<OrderShippedDomainEvent>
{
    public async Task Handle(DomainEventNotification<OrderShippedDomainEvent> notification, CancellationToken cancellationToken)
    {
        var domainEvent = notification.DomainEvent;

        await publishEndpoint.Publish(new OrderMarkedAsShippedEvent
        {
            CorrelationId = domainEvent.CorrelationId,
            OrderId = domainEvent.OrderId,
            TrackingNumber = domainEvent.TrackingNumber
        }, cancellationToken);
    }
}
