using OpenMind.BuildingBlocks.Application.Commands;

namespace OpenMind.Fulfillment.Application.Commands.CancelFulfillment;

public record CancelFulfillmentCommand : ICommand
{
    public Guid OrderId { get; init; }
    public Guid FulfillmentId { get; init; }
    public string Reason { get; init; } = string.Empty;
    public Guid CorrelationId { get; init; }
}
