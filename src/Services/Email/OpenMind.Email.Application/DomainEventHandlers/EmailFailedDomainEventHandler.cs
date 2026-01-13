using MassTransit;
using OpenMind.Email.IntegrationEvents.Events;
using OpenMind.Email.Domain.Events;
using OpenMind.Shared.Application.DomainEvents;

namespace OpenMind.Email.Application.DomainEventHandlers;

public class EmailFailedDomainEventHandler(IPublishEndpoint publishEndpoint)
    : IDomainEventHandler<EmailFailedDomainEvent>
{
    public async Task Handle(DomainEventNotification<EmailFailedDomainEvent> notification, CancellationToken cancellationToken)
    {
        var domainEvent = notification.DomainEvent;

        await publishEndpoint.Publish(new EmailFailedEvent
        {
            CorrelationId = domainEvent.CorrelationId,
            OrderId = domainEvent.OrderId,
            EmailType = domainEvent.EmailType,
            Reason = domainEvent.Reason
        }, cancellationToken);
    }
}
