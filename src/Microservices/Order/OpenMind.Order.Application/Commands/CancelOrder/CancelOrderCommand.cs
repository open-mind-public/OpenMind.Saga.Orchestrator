using OpenMind.Shared.Application.Commands;

namespace OpenMind.Order.Application.Commands.CancelOrder;

public record CancelOrderCommand : ICommand
{
    public Guid OrderId { get; init; }
    public string Reason { get; init; } = string.Empty;
    public Guid CorrelationId { get; init; }
}
