using OpenMind.Shared.IntegrationMessages;

namespace OpenMind.Email.Contract.Commands;

/// <summary>
/// Command to send order confirmation email.
/// </summary>
public record SendOrderConfirmationEmailCommand : IntegrationCommand, IEmailCommand
{
    public Guid OrderId { get; init; }
    public Guid CustomerId { get; init; }
    public string CustomerEmail { get; init; } = string.Empty;
    public string CustomerName { get; init; } = string.Empty;
    public decimal TotalAmount { get; init; }
    public string TrackingNumber { get; init; } = string.Empty;
}
