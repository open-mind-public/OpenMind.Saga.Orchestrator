using MassTransit;
using OpenMind.BuildingBlocks.Application.DomainEvents;
using OpenMind.BuildingBlocks.IntegrationEvents.Orders;
using OpenMind.Order.Domain.Events;

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
