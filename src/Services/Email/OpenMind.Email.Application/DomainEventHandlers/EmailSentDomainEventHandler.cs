using MassTransit;
using OpenMind.Email.IntegrationEvents.Events;
using OpenMind.Email.Domain.Events;
using OpenMind.Shared.Application.DomainEvents;

namespace OpenMind.Email.Application.DomainEventHandlers;

public class EmailSentDomainEventHandler(IPublishEndpoint publishEndpoint)
    : IDomainEventHandler<EmailSentDomainEvent>
{
    public async Task Handle(DomainEventNotification<EmailSentDomainEvent> notification, CancellationToken cancellationToken)
    {
        var domainEvent = notification.DomainEvent;

        await publishEndpoint.Publish(new EmailSentEvent
        {
            CorrelationId = domainEvent.CorrelationId,
            OrderId = domainEvent.OrderId,
            EmailType = domainEvent.EmailType,
            RecipientEmail = domainEvent.RecipientEmail
        }, cancellationToken);
    }
}
