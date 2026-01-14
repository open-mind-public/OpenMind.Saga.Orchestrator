using MassTransit;
using Microsoft.Extensions.Logging;
using OpenMind.Fulfillment.IntegrationEvents.Events;
using OpenMind.Fulfillment.Domain.Events;
using OpenMind.Shared.Application.DomainEvents;

namespace OpenMind.Fulfillment.Application.DomainEventHandlers;

public class FulfillmentShippedDomainEventHandler(IPublishEndpoint publishEndpoint, ILogger<FulfillmentShippedDomainEventHandler> logger)
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

        logger.LogInformation("[Fulfillment] Published OrderShippedEvent - OrderId: {OrderId}, TrackingNumber: {TrackingNumber}, CorrelationId: {CorrelationId}", domainEvent.OrderId, domainEvent.TrackingNumber, domainEvent.CorrelationId);
    }
}
