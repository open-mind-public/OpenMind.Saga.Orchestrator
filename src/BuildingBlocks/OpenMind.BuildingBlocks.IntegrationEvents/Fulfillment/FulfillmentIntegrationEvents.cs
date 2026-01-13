namespace OpenMind.BuildingBlocks.IntegrationEvents.Fulfillment;

// ========================================
// COMMANDS (Requests from Orchestrator)
// ========================================

/// <summary>
/// Command to fulfill an order (async call - shipping).
/// </summary>
public record FulfillOrderCommand : IntegrationEvent
{
    public Guid OrderId { get; init; }
    public Guid CustomerId { get; init; }
    public List<FulfillmentItemDto> Items { get; init; } = [];
    public string ShippingAddress { get; init; } = string.Empty;
}

/// <summary>
/// Command to cancel fulfillment.
/// </summary>
public record CancelFulfillmentCommand : IntegrationEvent
{
    public Guid OrderId { get; init; }
    public Guid FulfillmentId { get; init; }
    public string Reason { get; init; } = string.Empty;
}

// ========================================
// EVENTS (Responses to Orchestrator)
// ========================================

/// <summary>
/// Event indicating fulfillment was initiated.
/// </summary>
public record FulfillmentInitiatedEvent : IntegrationEvent
{
    public Guid OrderId { get; init; }
    public Guid FulfillmentId { get; init; }
    public string TrackingNumber { get; init; } = string.Empty;
}

/// <summary>
/// Event indicating order was shipped.
/// </summary>
public record OrderShippedEvent : IntegrationEvent
{
    public Guid OrderId { get; init; }
    public Guid FulfillmentId { get; init; }
    public string TrackingNumber { get; init; } = string.Empty;
    public DateTime EstimatedDelivery { get; init; }
}

/// <summary>
/// Event indicating fulfillment failed due to stock issues.
/// </summary>
public record FulfillmentFailedEvent : IntegrationEvent
{
    public Guid OrderId { get; init; }
    public string Reason { get; init; } = string.Empty;
    public List<OutOfStockItemDto> OutOfStockItems { get; init; } = [];
}

/// <summary>
/// Event indicating item is on backorder.
/// </summary>
public record ItemBackorderedEvent : IntegrationEvent
{
    public Guid OrderId { get; init; }
    public Guid FulfillmentId { get; init; }
    public List<BackorderItemDto> BackorderedItems { get; init; } = [];
    public DateTime EstimatedAvailability { get; init; }
}

/// <summary>
/// Event indicating fulfillment was cancelled.
/// </summary>
public record FulfillmentCancelledEvent : IntegrationEvent
{
    public Guid OrderId { get; init; }
    public Guid FulfillmentId { get; init; }
}

// ========================================
// DTOs
// ========================================

public record FulfillmentItemDto
{
    public Guid ProductId { get; init; }
    public string ProductName { get; init; } = string.Empty;
    public int Quantity { get; init; }
}

public record OutOfStockItemDto
{
    public Guid ProductId { get; init; }
    public string ProductName { get; init; } = string.Empty;
    public int RequestedQuantity { get; init; }
    public int AvailableQuantity { get; init; }
}

public record BackorderItemDto
{
    public Guid ProductId { get; init; }
    public string ProductName { get; init; } = string.Empty;
    public int Quantity { get; init; }
}
