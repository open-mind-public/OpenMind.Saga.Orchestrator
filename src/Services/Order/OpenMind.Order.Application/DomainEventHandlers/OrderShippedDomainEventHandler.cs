using MassTransit;
using OpenMind.BuildingBlocks.Application.DomainEvents;
using OpenMind.BuildingBlocks.IntegrationEvents.Orders;
using OpenMind.Order.Domain.Events;

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
