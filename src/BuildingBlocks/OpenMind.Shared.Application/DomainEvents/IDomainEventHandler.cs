using MediatR;
using OpenMind.Shared.Domain;

namespace OpenMind.Shared.Application.DomainEvents;

/// <summary>
/// Interface for domain event handlers.
/// </summary>
public interface IDomainEventHandler<TDomainEvent> : INotificationHandler<DomainEventNotification<TDomainEvent>>
    where TDomainEvent : IDomainEvent
{
}

/// <summary>
/// Wrapper for domain events to make them compatible with MediatR notifications.
/// </summary>
public record DomainEventNotification<TDomainEvent>(TDomainEvent DomainEvent) : INotification
    where TDomainEvent : IDomainEvent;
