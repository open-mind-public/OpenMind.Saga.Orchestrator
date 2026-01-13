using MassTransit;
using OpenMind.BuildingBlocks.Application.DomainEvents;
using OpenMind.BuildingBlocks.IntegrationEvents.Fulfillment;
using OpenMind.Fulfillment.Domain.Events;

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
