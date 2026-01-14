using OpenMind.Shared.IntegrationMessages;

namespace OpenMind.Email.IntegrationEvents.Events;

/// <summary>
/// Event indicating email sending failed.
/// </summary>
public record EmailFailedEvent : IntegrationEvent, IEmailEvent
{
    public Guid OrderId { get; init; }
    public string EmailType { get; init; } = string.Empty;
    public string Reason { get; init; } = string.Empty;
}
