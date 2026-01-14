using MassTransit;

namespace OpenMind.OrderPlacement.Orchestrator.Api.StateMachine;

/// <summary>
/// Saga state instance representing the order placement workflow state.
/// This is persisted to MongoDB to track the state of each order saga.
/// </summary>
public class OrderSagaState : SagaStateMachineInstance, ISagaVersion
{
    // MassTransit required property
    public Guid CorrelationId { get; set; }

    // ISagaVersion implementation for MongoDB repository
    public int Version { get; set; }

    // Current state of the saga
    public string CurrentState { get; set; } = string.Empty;

    // Order information
    public Guid OrderId { get; set; }
    public Guid CustomerId { get; set; }
    public decimal TotalAmount { get; set; }
    public string ShippingAddress { get; set; } = string.Empty;

    // Customer information for email notifications
    public string CustomerEmail { get; set; } = string.Empty;
    public string CustomerName { get; set; } = string.Empty;

    // Order items (serialized)
    public string OrderItemsJson { get; set; } = "[]";

    // Payment information
    public Guid? PaymentId { get; set; }
    public string? PaymentTransactionId { get; set; }

    // Fulfillment information
    public Guid? FulfillmentId { get; set; }
    public string? TrackingNumber { get; set; }
    public DateTime? EstimatedDelivery { get; set; }

    // Error tracking
    public string? LastError { get; set; }
    public string? LastErrorCode { get; set; }
    public int RetryCount { get; set; }

    // Timestamps
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
}
