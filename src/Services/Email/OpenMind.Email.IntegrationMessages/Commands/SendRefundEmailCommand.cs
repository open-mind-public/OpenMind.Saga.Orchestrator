using OpenMind.Shared.IntegrationMessages;

namespace OpenMind.Email.IntegrationEvents.Commands;

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
