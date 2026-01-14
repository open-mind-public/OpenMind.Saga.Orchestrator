using MassTransit;
using Microsoft.Extensions.Logging;
using OpenMind.Payment.Contract.Events;
using OpenMind.Payment.Domain.Events;
using OpenMind.Shared.Application.DomainEvents;

namespace OpenMind.Payment.Application.DomainEventHandlers;

/// <summary>
/// Handles the PaymentFailedDomainEvent.
/// Publishes the PaymentFailedEvent integration event to notify other services.
/// </summary>
public class PaymentFailedDomainEventHandler(IPublishEndpoint publishEndpoint, ILogger<PaymentFailedDomainEventHandler> logger)
    : IDomainEventHandler<PaymentFailedDomainEvent>
{
    public async Task Handle(DomainEventNotification<PaymentFailedDomainEvent> notification, CancellationToken cancellationToken)
    {
        var domainEvent = notification.DomainEvent;

        await publishEndpoint.Publish(new PaymentFailedEvent
        {
            OrderId = domainEvent.OrderId,
            Reason = domainEvent.Reason,
            ErrorCode = "PAYMENT_DECLINED"
        }, cancellationToken);

        logger.LogWarning("[Payment] Published PaymentFailedEvent - OrderId: {OrderId}, Reason: {Reason}", domainEvent.OrderId, domainEvent.Reason);
    }
}
