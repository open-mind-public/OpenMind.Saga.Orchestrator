using MassTransit;
using Microsoft.Extensions.Logging;
using OpenMind.Fulfillment.Contract.Events;
using OpenMind.Fulfillment.Domain.Events;
using OpenMind.Shared.Application.DomainEvents;

namespace OpenMind.Fulfillment.Application.DomainEventHandlers;

public class FulfillmentBackOrderedDomainEventHandler(IPublishEndpoint publishEndpoint, ILogger<FulfillmentBackOrderedDomainEventHandler> logger)
    : IDomainEventHandler<FulfillmentBackOrderedDomainEvent>
{
    public async Task Handle(DomainEventNotification<FulfillmentBackOrderedDomainEvent> notification, CancellationToken cancellationToken)
    {
        var domainEvent = notification.DomainEvent;

        await publishEndpoint.Publish(new FulfillmentFailedEvent
        {
            CorrelationId = domainEvent.CorrelationId,
            OrderId = domainEvent.OrderId,
            Reason = domainEvent.Reason,
            OutOfStockItems = [] // Items info not available from domain event - consider enriching
        }, cancellationToken);

        logger.LogWarning(
            "[Fulfillment] Published FulfillmentFailedEvent - OrderId: {OrderId}, Reason: {Reason}, CorrelationId: {CorrelationId}",
            domainEvent.OrderId,
            domainEvent.Reason,
            domainEvent.CorrelationId);
    }
}
