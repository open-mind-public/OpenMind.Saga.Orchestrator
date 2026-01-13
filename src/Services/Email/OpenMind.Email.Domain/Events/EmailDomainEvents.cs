using OpenMind.Shared.Domain;

namespace OpenMind.Email.Domain.Events;

public record EmailSentDomainEvent(Guid EmailId, Guid OrderId, string EmailType, string RecipientEmail, Guid CorrelationId) : DomainEvent;

public record EmailFailedDomainEvent(Guid EmailId, Guid OrderId, string EmailType, string Reason, Guid CorrelationId) : DomainEvent;
