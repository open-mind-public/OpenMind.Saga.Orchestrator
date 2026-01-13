using OpenMind.BuildingBlocks.Application.Commands;

namespace OpenMind.Order.Application.Commands.MarkOrderAsPaymentCompleted;

public record MarkOrderAsPaymentCompletedCommand : ICommand
{
    public Guid OrderId { get; init; }
    public string TransactionId { get; init; } = string.Empty;
    public Guid CorrelationId { get; init; }
}
