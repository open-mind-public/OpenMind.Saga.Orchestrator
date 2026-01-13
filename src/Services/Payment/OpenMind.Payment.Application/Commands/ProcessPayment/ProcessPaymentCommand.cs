using OpenMind.Shared.Application.Commands;

namespace OpenMind.Payment.Application.Commands.ProcessPayment;

public record ProcessPaymentCommand : ICommand<Guid>
{
    public Guid OrderId { get; init; }
    public Guid CustomerId { get; init; }
    public decimal Amount { get; init; }
    public string PaymentMethod { get; init; } = string.Empty;
    public string CardNumber { get; init; } = string.Empty;
    public string CardExpiry { get; init; } = string.Empty;
}
