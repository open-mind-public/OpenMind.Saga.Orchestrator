using OpenMind.BuildingBlocks.Domain;

namespace OpenMind.Payment.Domain.Events;

public record PaymentCreatedDomainEvent(Guid PaymentId, Guid OrderId, decimal Amount) : DomainEvent;

public record PaymentCompletedDomainEvent(Guid PaymentId, Guid OrderId, string TransactionId) : DomainEvent;

public record PaymentFailedDomainEvent(Guid PaymentId, Guid OrderId, string Reason) : DomainEvent;

public record PaymentRefundedDomainEvent(Guid PaymentId, Guid OrderId, decimal Amount) : DomainEvent;
