using MassTransit;
using OpenMind.BuildingBlocks.Application.DomainEvents;
using OpenMind.BuildingBlocks.IntegrationEvents.Fulfillment;
using OpenMind.Fulfillment.Domain.Events;

namespace OpenMind.Fulfillment.Application.DomainEventHandlers;

public class FulfillmentCancelledDomainEventHandler(IPublishEndpoint publishEndpoint)
    : IDomainEventHandler<FulfillmentCancelledDomainEvent>
{
    public async Task Handle(DomainEventNotification<FulfillmentCancelledDomainEvent> notification, CancellationToken cancellationToken)
    {
        var domainEvent = notification.DomainEvent;

        await publishEndpoint.Publish(new FulfillmentCancelledEvent
        {
            CorrelationId = domainEvent.CorrelationId,
            OrderId = domainEvent.OrderId,
            FulfillmentId = domainEvent.FulfillmentId
        }, cancellationToken);
    }
}
