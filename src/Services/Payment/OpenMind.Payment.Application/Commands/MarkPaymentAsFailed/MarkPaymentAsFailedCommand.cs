using OpenMind.BuildingBlocks.Application.Commands;

namespace OpenMind.Payment.Application.Commands.MarkPaymentAsFailed;

public record MarkPaymentAsFailedCommand : ICommand<bool>
{
    public Guid PaymentId { get; init; }
    public string Reason { get; init; } = string.Empty;
}
