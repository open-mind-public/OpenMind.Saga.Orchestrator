using OpenMind.Shared.Application.Commands;

namespace OpenMind.Order.Application.Commands.MarkOrderAsBackOrdered;

public record MarkOrderAsBackOrderedCommand : ICommand
{
    public Guid OrderId { get; init; }
    public string Reason { get; init; } = string.Empty;
    public Guid CorrelationId { get; init; }
}
