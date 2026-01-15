using OpenMind.Shared.Domain;

namespace OpenMind.Fulfillment.Domain.Events;

public record FulfillmentCreatedDomainEvent(Guid FulfillmentId, Guid OrderId) : DomainEvent;

public record FulfillmentShippedDomainEvent(Guid FulfillmentId, Guid OrderId, string TrackingNumber, DateTime EstimatedDelivery, Guid CorrelationId) : DomainEvent;

public record FulfillmentBackOrderedDomainEvent(Guid FulfillmentId, Guid OrderId, string Reason, Guid CorrelationId) : DomainEvent;

public record FulfillmentCancelledDomainEvent(Guid FulfillmentId, Guid OrderId, Guid CorrelationId) : DomainEvent;
