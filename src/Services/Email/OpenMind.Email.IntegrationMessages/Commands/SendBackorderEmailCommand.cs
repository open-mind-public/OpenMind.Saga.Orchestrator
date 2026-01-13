using OpenMind.Shared.IntegrationMessages;

namespace OpenMind.Email.IntegrationEvents.Commands;

/// <summary>
/// Command to send backorder notification email.
/// </summary>
public record SendBackorderEmailCommand : IntegrationCommand
{
    public Guid OrderId { get; init; }
    public Guid CustomerId { get; init; }
    public string CustomerEmail { get; init; } = string.Empty;
    public string CustomerName { get; init; } = string.Empty;
    public List<string> BackorderedProducts { get; init; } = [];
    public DateTime EstimatedAvailability { get; init; }
}
