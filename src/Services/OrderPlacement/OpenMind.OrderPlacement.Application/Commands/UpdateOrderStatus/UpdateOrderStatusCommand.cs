using OpenMind.BuildingBlocks.Application.Commands;

namespace OpenMind.OrderPlacement.Application.Commands.UpdateOrderStatus;

public record UpdateOrderStatusCommand : ICommand
{
    public Guid OrderId { get; init; }
    public string Status { get; init; } = string.Empty;
    public string? Reason { get; init; }
}
