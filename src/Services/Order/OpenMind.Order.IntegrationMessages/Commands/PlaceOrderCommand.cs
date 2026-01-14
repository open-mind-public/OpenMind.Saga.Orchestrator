using OpenMind.Shared.IntegrationMessages;

namespace OpenMind.Order.IntegrationEvents.Commands;

/// <summary>
/// Command to initiate the order placement saga.
/// The order must already exist in the Order Service.
/// </summary>
public record PlaceOrderCommand : IntegrationCommand, IOrderCommand
{
    /// <summary>
    /// The ID of an existing order to be placed.
    /// </summary>
    public Guid OrderId { get; init; }
}
