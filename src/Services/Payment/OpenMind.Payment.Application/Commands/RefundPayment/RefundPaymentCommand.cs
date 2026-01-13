using OpenMind.BuildingBlocks.Application.Commands;

namespace OpenMind.Payment.Application.Commands.RefundPayment;

public record RefundPaymentCommand : ICommand
{
    public Guid OrderId { get; init; }
    public Guid PaymentId { get; init; }
    public decimal Amount { get; init; }
    public string Reason { get; init; } = string.Empty;
    public Guid CorrelationId { get; init; }
}
