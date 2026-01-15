using MassTransit;
using MongoDB.Bson.Serialization.Attributes;

namespace OpenMind.OrderPlacement.Orchestrator.Api;

public class OrderSagaState : SagaStateMachineInstance, ISagaVersion
{
    [BsonId]
    public Guid CorrelationId { get; set; }
    public int Version { get; set; }
    public string CurrentState { get; set; } = string.Empty;

    public Guid OrderId { get; set; }
    public Guid CustomerId { get; set; }
    public decimal TotalAmount { get; set; }
    public string ShippingAddress { get; set; } = string.Empty;

    public string CustomerEmail { get; set; } = string.Empty;
    public string CustomerName { get; set; } = string.Empty;

    public string OrderItemsJson { get; set; } = "[]";

    public Guid? PaymentId { get; set; }
    public string? PaymentTransactionId { get; set; }

    public Guid? FulfillmentId { get; set; }
    public string? TrackingNumber { get; set; }
    public DateTime? EstimatedDelivery { get; set; }

    public string? LastError { get; set; }
    public string? LastErrorCode { get; set; }
    public int RetryCount { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
}
