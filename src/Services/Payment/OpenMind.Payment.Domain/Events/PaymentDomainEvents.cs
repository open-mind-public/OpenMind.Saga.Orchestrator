using OpenMind.Shared.Domain;

namespace OpenMind.Payment.Domain.Events;

public record PaymentCreatedDomainEvent(Guid PaymentId, Guid OrderId, decimal Amount) : DomainEvent;

public record PaymentProcessingStartedDomainEvent(
    Guid PaymentId, 
    Guid OrderId, 
    decimal Amount,
    string CardNumber,
    string CardExpiry) : DomainEvent;

public record PaymentPaidDomainEvent(Guid PaymentId, Guid OrderId, decimal Amount, string TransactionId) : DomainEvent;

public record PaymentCompletedDomainEvent(Guid PaymentId, Guid OrderId, string TransactionId) : DomainEvent;

public record PaymentFailedDomainEvent(Guid PaymentId, Guid OrderId, string Reason) : DomainEvent;

public record PaymentRefundedDomainEvent(Guid PaymentId, Guid OrderId, decimal Amount, Guid CorrelationId) : DomainEvent;

public record PaymentRefundFailedDomainEvent(Guid PaymentId, Guid OrderId, string Reason, Guid CorrelationId) : DomainEvent;
