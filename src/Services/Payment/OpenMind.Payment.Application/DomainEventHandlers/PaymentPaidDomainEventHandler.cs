using MassTransit;
using OpenMind.BuildingBlocks.Application.DomainEvents;
using OpenMind.BuildingBlocks.IntegrationEvents.Payments;
using OpenMind.Payment.Domain.Events;

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
