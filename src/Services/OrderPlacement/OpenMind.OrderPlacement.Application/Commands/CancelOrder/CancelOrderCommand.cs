using OpenMind.BuildingBlocks.Application.Commands;

namespace OpenMind.OrderPlacement.Application.Commands.CancelOrder;

public record CancelOrderCommand : ICommand
{
    public Guid OrderId { get; init; }
    public string Reason { get; init; } = string.Empty;
}
