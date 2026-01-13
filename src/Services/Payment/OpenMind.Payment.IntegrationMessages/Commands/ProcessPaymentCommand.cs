using OpenMind.Shared.IntegrationMessages;

namespace OpenMind.Payment.IntegrationEvents.Commands;

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
