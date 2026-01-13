using MassTransit;
using OpenMind.BuildingBlocks.Application.DomainEvents;
using OpenMind.BuildingBlocks.IntegrationEvents.Orders;
using OpenMind.Order.Domain.Events;

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
