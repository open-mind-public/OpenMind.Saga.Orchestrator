using MassTransit;
using OpenMind.Fulfillment.IntegrationEvents.Events;
using OpenMind.Fulfillment.Domain.Events;
using OpenMind.Shared.Application.DomainEvents;

namespace OpenMind.Fulfillment.Application.DomainEventHandlers;

public class FulfillmentBackOrderedDomainEventHandler(IPublishEndpoint publishEndpoint)
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
    }
}
