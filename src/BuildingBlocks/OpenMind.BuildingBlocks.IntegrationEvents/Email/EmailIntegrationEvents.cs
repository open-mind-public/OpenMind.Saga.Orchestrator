namespace OpenMind.BuildingBlocks.IntegrationEvents.Email;

// ========================================
// COMMANDS (Requests from Orchestrator)
// ========================================

/// <summary>
/// Command to send order confirmation email.
/// </summary>
public record SendOrderConfirmationEmailCommand : IntegrationCommand
{
    public Guid OrderId { get; init; }
    public Guid CustomerId { get; init; }
    public string CustomerEmail { get; init; } = string.Empty;
    public string CustomerName { get; init; } = string.Empty;
    public decimal TotalAmount { get; init; }
    public string TrackingNumber { get; init; } = string.Empty;
}

/// <summary>
/// Command to send payment failure notification email.
/// </summary>
public record SendPaymentFailedEmailCommand : IntegrationCommand
{
    public Guid OrderId { get; init; }
    public Guid CustomerId { get; init; }
    public string CustomerEmail { get; init; } = string.Empty;
    public string CustomerName { get; init; } = string.Empty;
    public string FailureReason { get; init; } = string.Empty;
}

/// <summary>
/// Command to send order cancellation email.
/// </summary>
public record SendOrderCancelledEmailCommand : IntegrationCommand
{
    public Guid OrderId { get; init; }
    public Guid CustomerId { get; init; }
    public string CustomerEmail { get; init; } = string.Empty;
    public string CustomerName { get; init; } = string.Empty;
    public string CancellationReason { get; init; } = string.Empty;
}

/// <summary>
/// Command to send backorder notification email.
/// </summary>
public record SendBackorderEmailCommand : IntegrationCommand
{
    public Guid OrderId { get; init; }
    public Guid CustomerId { get; init; }
    public string CustomerEmail { get; init; } = string.Empty;
    public string CustomerName { get; init; } = string.Empty;
    public List<string> BackorderedProducts { get; init; } = [];
    public DateTime EstimatedAvailability { get; init; }
}

/// <summary>
/// Command to send refund confirmation email.
/// </summary>
public record SendRefundEmailCommand : IntegrationCommand
{
    public Guid OrderId { get; init; }
    public Guid CustomerId { get; init; }
    public string CustomerEmail { get; init; } = string.Empty;
    public string CustomerName { get; init; } = string.Empty;
    public decimal RefundAmount { get; init; }
}

// ========================================
// EVENTS (Responses to Orchestrator)
// ========================================

/// <summary>
/// Event indicating email was sent successfully.
/// </summary>
public record EmailSentEvent : IntegrationEvent
{
    public Guid OrderId { get; init; }
    public string EmailType { get; init; } = string.Empty;
    public string RecipientEmail { get; init; } = string.Empty;
}

/// <summary>
/// Event indicating email sending failed.
/// </summary>
public record EmailFailedEvent : IntegrationEvent
{
    public Guid OrderId { get; init; }
    public string EmailType { get; init; } = string.Empty;
    public string Reason { get; init; } = string.Empty;
}
