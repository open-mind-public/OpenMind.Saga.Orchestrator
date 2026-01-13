namespace OpenMind.BuildingBlocks.IntegrationEvents.Payments;

// ========================================
// COMMANDS (Requests from Orchestrator)
// ========================================

/// <summary>
/// Command to process payment for an order.
/// </summary>
public record ProcessPaymentCommand : IntegrationCommand
{
    public Guid OrderId { get; init; }
    public Guid CustomerId { get; init; }
    public decimal Amount { get; init; }
    public string PaymentMethod { get; init; } = string.Empty;
    public string CardNumber { get; init; } = string.Empty;
    public string CardExpiry { get; init; } = string.Empty;
}

/// <summary>
/// Command to refund payment.
/// </summary>
public record RefundPaymentCommand : IntegrationCommand
{
    public Guid OrderId { get; init; }
    public Guid PaymentId { get; init; }
    public decimal Amount { get; init; }
    public string Reason { get; init; } = string.Empty;
}

// ========================================
// EVENTS (Responses to Orchestrator)
// ========================================

/// <summary>
/// Event indicating payment was successfully processed.
/// </summary>
public record PaymentCompletedEvent : IntegrationEvent
{
    public Guid OrderId { get; init; }
    public Guid PaymentId { get; init; }
    public decimal Amount { get; init; }
    public string TransactionId { get; init; } = string.Empty;
}

/// <summary>
/// Event indicating payment processing failed.
/// </summary>
public record PaymentFailedEvent : IntegrationEvent
{
    public Guid OrderId { get; init; }
    public string Reason { get; init; } = string.Empty;
    public string ErrorCode { get; init; } = string.Empty;
}

/// <summary>
/// Event indicating payment was refunded.
/// </summary>
public record PaymentRefundedEvent : IntegrationEvent
{
    public Guid OrderId { get; init; }
    public Guid PaymentId { get; init; }
    public decimal Amount { get; init; }
}

/// <summary>
/// Event indicating payment refund failed.
/// </summary>
public record PaymentRefundFailedEvent : IntegrationEvent
{
    public Guid OrderId { get; init; }
    public string Reason { get; init; } = string.Empty;
}
