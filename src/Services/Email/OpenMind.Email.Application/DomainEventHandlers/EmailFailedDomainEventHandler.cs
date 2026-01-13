using MassTransit;
using OpenMind.BuildingBlocks.Application.DomainEvents;
using OpenMind.BuildingBlocks.IntegrationEvents.Email;
using OpenMind.Email.Domain.Events;

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
