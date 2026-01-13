using OpenMind.BuildingBlocks.Domain;

namespace OpenMind.OrderPlacement.Domain.Events;

public record OrderCreatedDomainEvent(Guid OrderId, Guid CustomerId) : DomainEvent;

public record OrderItemAddedDomainEvent(Guid OrderId, Guid ProductId, int Quantity) : DomainEvent;

public record OrderPaymentCompletedDomainEvent(Guid OrderId, string TransactionId) : DomainEvent;

public record OrderPaymentFailedDomainEvent(Guid OrderId, string Reason) : DomainEvent;

public record OrderShippedDomainEvent(Guid OrderId, string TrackingNumber) : DomainEvent;

public record OrderBackOrderedDomainEvent(Guid OrderId, string Reason) : DomainEvent;

public record OrderCancelledDomainEvent(Guid OrderId, string Reason) : DomainEvent;

public record OrderRefundedDomainEvent(Guid OrderId) : DomainEvent;
