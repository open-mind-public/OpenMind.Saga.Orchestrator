using MassTransit;
using OpenMind.Fulfillment.IntegrationEvents.Events;
using OpenMind.Fulfillment.Domain.Events;
using OpenMind.Shared.Application.DomainEvents;

namespace OpenMind.Fulfillment.Application.DomainEventHandlers;

public class FulfillmentShippedDomainEventHandler(IPublishEndpoint publishEndpoint)
    : IDomainEventHandler<FulfillmentShippedDomainEvent>
{
    public async Task Handle(DomainEventNotification<FulfillmentShippedDomainEvent> notification, CancellationToken cancellationToken)
    {
        var domainEvent = notification.DomainEvent;

        await publishEndpoint.Publish(new OrderShippedEvent
        {
            CorrelationId = domainEvent.CorrelationId,
            OrderId = domainEvent.OrderId,
            FulfillmentId = domainEvent.FulfillmentId,
            TrackingNumber = domainEvent.TrackingNumber,
            EstimatedDelivery = domainEvent.EstimatedDelivery
        }, cancellationToken);
    }
}
