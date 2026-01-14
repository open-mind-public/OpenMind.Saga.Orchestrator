using MassTransit;
using Microsoft.Extensions.Logging;
using OpenMind.Payment.IntegrationEvents.Events;
using OpenMind.Payment.Domain.Events;
using OpenMind.Shared.Application.DomainEvents;

namespace OpenMind.Payment.Application.DomainEventHandlers;

public class PaymentPaidDomainEventHandler(IPublishEndpoint publishEndpoint, ILogger<PaymentPaidDomainEventHandler> logger)
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

        logger.LogInformation("[Payment] Published PaymentCompletedEvent - OrderId: {OrderId}, PaymentId: {PaymentId}, Amount: {Amount}", domainEvent.OrderId, domainEvent.PaymentId, domainEvent.Amount);
    }
}
