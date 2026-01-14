using OpenMind.Shared.IntegrationMessages;

namespace OpenMind.Fulfillment.Contract.Commands;

/// <summary>
/// Command to cancel fulfillment.
/// </summary>
public record CancelFulfillmentCommand : IntegrationCommand, IFulfillmentCommand
{
    public Guid OrderId { get; init; }
    public Guid FulfillmentId { get; init; }
    public string Reason { get; init; } = string.Empty;
}
