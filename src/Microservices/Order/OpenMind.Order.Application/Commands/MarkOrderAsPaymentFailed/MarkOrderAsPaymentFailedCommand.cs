using OpenMind.Shared.Application.Commands;

namespace OpenMind.Order.Application.Commands.MarkOrderAsPaymentFailed;

public record MarkOrderAsPaymentFailedCommand : ICommand
{
    public Guid OrderId { get; init; }
    public string Reason { get; init; } = string.Empty;
    public Guid CorrelationId { get; init; }
}
