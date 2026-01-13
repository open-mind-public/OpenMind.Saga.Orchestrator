using MassTransit;
using OpenMind.Fulfillment.IntegrationEvents.Events;
using OpenMind.Fulfillment.Domain.Events;
using OpenMind.Shared.Application.DomainEvents;

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
