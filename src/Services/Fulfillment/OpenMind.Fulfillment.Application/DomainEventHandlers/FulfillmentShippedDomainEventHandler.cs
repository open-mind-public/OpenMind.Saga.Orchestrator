using MassTransit;
using OpenMind.BuildingBlocks.Application.DomainEvents;
using OpenMind.BuildingBlocks.IntegrationEvents.Fulfillment;
using OpenMind.Fulfillment.Domain.Events;

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
