using OpenMind.BuildingBlocks.Application.Commands;

namespace OpenMind.Order.Application.Commands.MarkOrderAsShipped;

public record MarkOrderAsShippedCommand : ICommand
{
    public Guid OrderId { get; init; }
    public string TrackingNumber { get; init; } = string.Empty;
    public Guid CorrelationId { get; init; }
}
