using OpenMind.Shared.Application.Commands;

namespace OpenMind.Payment.Application.Commands.RetryPayment;

public record RetryPaymentCommand : ICommand<bool>
{
    public Guid OrderId { get; init; }
}
