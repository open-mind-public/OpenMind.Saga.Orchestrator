using OpenMind.Shared.IntegrationMessages;

namespace OpenMind.Order.IntegrationEvents.Commands;

/// <summary>
/// Command to validate an existing order for placement.
/// The order is assumed to already exist in the Order Service.
/// </summary>
public record ValidateOrderCommand : IntegrationCommand
{
    public Guid OrderId { get; init; }
}
