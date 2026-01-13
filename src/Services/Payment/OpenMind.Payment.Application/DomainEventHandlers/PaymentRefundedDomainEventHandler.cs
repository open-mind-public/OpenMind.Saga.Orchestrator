using MassTransit;
using OpenMind.BuildingBlocks.Application.DomainEvents;
using OpenMind.BuildingBlocks.IntegrationEvents.Payments;
using OpenMind.Payment.Domain.Events;

namespace OpenMind.Payment.Application.DomainEventHandlers;

public class PaymentRefundedDomainEventHandler(IPublishEndpoint publishEndpoint)
    : IDomainEventHandler<PaymentRefundedDomainEvent>
{
    public async Task Handle(DomainEventNotification<PaymentRefundedDomainEvent> notification, CancellationToken cancellationToken)
    {
        var domainEvent = notification.DomainEvent;

        await publishEndpoint.Publish(new PaymentRefundedEvent
        {
            CorrelationId = domainEvent.CorrelationId,
            OrderId = domainEvent.OrderId,
            PaymentId = domainEvent.PaymentId,
            Amount = domainEvent.Amount
        }, cancellationToken);
    }
}
