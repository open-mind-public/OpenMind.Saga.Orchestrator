using OpenMind.Shared.Application.Commands;

namespace OpenMind.Payment.Application.Commands.MarkPaymentAsPaid;

public record MarkPaymentAsPaidCommand : ICommand<bool>
{
    public Guid PaymentId { get; init; }
    public string TransactionId { get; init; } = string.Empty;
}
