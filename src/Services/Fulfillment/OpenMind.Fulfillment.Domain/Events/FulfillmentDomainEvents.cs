using OpenMind.BuildingBlocks.Domain;

namespace OpenMind.Fulfillment.Domain.Events;

public record FulfillmentCreatedDomainEvent(Guid FulfillmentId, Guid OrderId) : DomainEvent;

public record FulfillmentShippedDomainEvent(Guid FulfillmentId, Guid OrderId, string TrackingNumber) : DomainEvent;

public record FulfillmentBackOrderedDomainEvent(Guid FulfillmentId, Guid OrderId, string Reason) : DomainEvent;

public record FulfillmentCancelledDomainEvent(Guid FulfillmentId, Guid OrderId) : DomainEvent;
