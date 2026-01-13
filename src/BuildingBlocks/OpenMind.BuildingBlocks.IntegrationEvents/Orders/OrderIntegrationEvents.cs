namespace OpenMind.BuildingBlocks.IntegrationEvents.Orders;

// ========================================
// COMMANDS (Requests from Orchestrator)
// ========================================

/// <summary>
/// Command to create a new order.
/// </summary>
public record CreateOrderCommand : IntegrationEvent
{
    public Guid OrderId { get; init; }
    public Guid CustomerId { get; init; }
    public List<OrderItemDto> Items { get; init; } = [];
    public decimal TotalAmount { get; init; }
    public string ShippingAddress { get; init; } = string.Empty;
}

/// <summary>
/// Command to update order status.
/// </summary>
public record UpdateOrderStatusCommand : IntegrationEvent
{
    public Guid OrderId { get; init; }
    public string Status { get; init; } = string.Empty;
    public string? Reason { get; init; }
}

/// <summary>
/// Command to cancel an order.
/// </summary>
public record CancelOrderCommand : IntegrationEvent
{
    public Guid OrderId { get; init; }
    public string Reason { get; init; } = string.Empty;
}

// ========================================
// EVENTS (Responses to Orchestrator)
// ========================================

/// <summary>
/// Event indicating order was successfully created.
/// </summary>
public record OrderCreatedEvent : IntegrationEvent
{
    public Guid OrderId { get; init; }
    public Guid CustomerId { get; init; }
    public decimal TotalAmount { get; init; }
}

/// <summary>
/// Event indicating order creation failed.
/// </summary>
public record OrderCreationFailedEvent : IntegrationEvent
{
    public Guid OrderId { get; init; }
    public string Reason { get; init; } = string.Empty;
}

/// <summary>
/// Event indicating order status was updated.
/// </summary>
public record OrderStatusUpdatedEvent : IntegrationEvent
{
    public Guid OrderId { get; init; }
    public string Status { get; init; } = string.Empty;
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
