namespace OpenMind.BuildingBlocks.IntegrationEvents.Orders;

// ========================================
// COMMANDS (Requests from Orchestrator)
// ========================================

/// <summary>
/// Command to validate an existing order for placement.
/// The order is assumed to already exist in the Order Service.
/// </summary>
public record ValidateOrderCommand : IntegrationCommand
{
    public Guid OrderId { get; init; }
}

/// <summary>
/// Command to mark order payment as completed.
/// </summary>
public record MarkOrderAsPaymentCompletedCommand : IntegrationCommand
{
    public Guid OrderId { get; init; }
    public string TransactionId { get; init; } = string.Empty;
}

/// <summary>
/// Command to mark order payment as failed.
/// </summary>
public record MarkOrderAsPaymentFailedCommand : IntegrationCommand
{
    public Guid OrderId { get; init; }
    public string Reason { get; init; } = string.Empty;
}

/// <summary>
/// Command to mark order as shipped.
/// </summary>
public record MarkOrderAsShippedCommand : IntegrationCommand
{
    public Guid OrderId { get; init; }
    public string TrackingNumber { get; init; } = string.Empty;
}

/// <summary>
/// Command to mark order as back ordered.
/// </summary>
public record MarkOrderAsBackOrderedCommand : IntegrationCommand
{
    public Guid OrderId { get; init; }
    public string Reason { get; init; } = string.Empty;
}

/// <summary>
/// Command to cancel an order.
/// </summary>
public record CancelOrderCommand : IntegrationCommand
{
    public Guid OrderId { get; init; }
    public string Reason { get; init; } = string.Empty;
}

// ========================================
// EVENTS (Responses to Orchestrator)
// ========================================

/// <summary>
/// Event indicating order was validated and ready for placement.
/// Contains order details retrieved from the Order Service.
/// </summary>
public record OrderValidatedEvent : IntegrationEvent
{
    public Guid OrderId { get; init; }
    public Guid CustomerId { get; init; }
    public decimal TotalAmount { get; init; }
    public string ShippingAddress { get; init; } = string.Empty;
    public string CustomerEmail { get; init; } = string.Empty;
    public string CustomerName { get; init; } = string.Empty;
    public List<OrderItemDto> Items { get; init; } = [];
}

/// <summary>
/// Event indicating order validation failed.
/// </summary>
public record OrderValidationFailedEvent : IntegrationEvent
{
    public Guid OrderId { get; init; }
    public string Reason { get; init; } = string.Empty;
}

/// <summary>
/// Event indicating order payment was marked as completed.
/// </summary>
public record OrderPaymentCompletedEvent : IntegrationEvent
{
    public Guid OrderId { get; init; }
    public string TransactionId { get; init; } = string.Empty;
}

/// <summary>
/// Event indicating order payment was marked as failed.
/// </summary>
public record OrderPaymentFailedEvent : IntegrationEvent
{
    public Guid OrderId { get; init; }
    public string Reason { get; init; } = string.Empty;
}

/// <summary>
/// Event indicating order status was marked as shipped.
/// </summary>
public record OrderMarkedAsShippedEvent : IntegrationEvent
{
    public Guid OrderId { get; init; }
    public string TrackingNumber { get; init; } = string.Empty;
}

/// <summary>
/// Event indicating order was marked as back ordered.
/// </summary>
public record OrderBackOrderedEvent : IntegrationEvent
{
    public Guid OrderId { get; init; }
    public string Reason { get; init; } = string.Empty;
}

/// <summary>
/// Event indicating order was cancelled.
/// </summary>
public record OrderCancelledEvent : IntegrationEvent
{
    public Guid OrderId { get; init; }
}

// ========================================
// DTOs
// ========================================

public record OrderItemDto
{
    public Guid ProductId { get; init; }
    public string ProductName { get; init; } = string.Empty;
    public int Quantity { get; init; }
    public decimal UnitPrice { get; init; }
}
