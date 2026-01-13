using MassTransit;
using OpenMind.BuildingBlocks.Application.DomainEvents;
using OpenMind.BuildingBlocks.IntegrationEvents.Orders;
using OpenMind.Order.Domain.Events;

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
