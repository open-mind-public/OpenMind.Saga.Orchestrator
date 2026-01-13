using MassTransit;
using OpenMind.BuildingBlocks.Application.DomainEvents;
using OpenMind.BuildingBlocks.IntegrationEvents.Orders;
using OpenMind.Order.Domain.Events;

namespace OpenMind.Order.Application.DomainEventHandlers;

public class OrderBackOrderedDomainEventHandler(IPublishEndpoint publishEndpoint)
    : IDomainEventHandler<OrderBackOrderedDomainEvent>
{
    public async Task Handle(DomainEventNotification<OrderBackOrderedDomainEvent> notification, CancellationToken cancellationToken)
    {
        var domainEvent = notification.DomainEvent;

        await publishEndpoint.Publish(new OrderBackOrderedEvent
        {
            CorrelationId = domainEvent.CorrelationId,
            OrderId = domainEvent.OrderId,
            Reason = domainEvent.Reason
        }, cancellationToken);
    }
}
