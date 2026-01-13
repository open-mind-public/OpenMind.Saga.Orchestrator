using MassTransit;
using OpenMind.Payment.IntegrationEvents.Events;
using OpenMind.Payment.Domain.Events;
using OpenMind.Shared.Application.DomainEvents;

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
