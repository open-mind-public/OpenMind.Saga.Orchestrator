using MassTransit;
using OpenMind.Payment.Contract.Events;
using OpenMind.Payment.Domain.Events;
using OpenMind.Shared.Application.DomainEvents;

namespace OpenMind.Payment.Application.DomainEventHandlers;

public class PaymentRefundFailedDomainEventHandler(IPublishEndpoint publishEndpoint)
    : IDomainEventHandler<PaymentRefundFailedDomainEvent>
{
    public async Task Handle(DomainEventNotification<PaymentRefundFailedDomainEvent> notification, CancellationToken cancellationToken)
    {
        var domainEvent = notification.DomainEvent;

        await publishEndpoint.Publish(new PaymentRefundFailedEvent
        {
            CorrelationId = domainEvent.CorrelationId,
            OrderId = domainEvent.OrderId,
            Reason = domainEvent.Reason
        }, cancellationToken);
    }
}
