using MassTransit;
using OpenMind.Payment.IntegrationEvents.Events;
using OpenMind.Payment.Domain.Events;
using OpenMind.Shared.Application.DomainEvents;

namespace OpenMind.Payment.Application.DomainEventHandlers;

public class PaymentPaidDomainEventHandler(IPublishEndpoint publishEndpoint)
    : IDomainEventHandler<PaymentPaidDomainEvent>
{
    public async Task Handle(DomainEventNotification<PaymentPaidDomainEvent> notification, CancellationToken cancellationToken)
    {
        var domainEvent = notification.DomainEvent;

        await publishEndpoint.Publish(new PaymentCompletedEvent
        {
            OrderId = domainEvent.OrderId,
            PaymentId = domainEvent.PaymentId,
            Amount = domainEvent.Amount,
            TransactionId = domainEvent.TransactionId
        }, cancellationToken);
    }
}
