using OpenMind.Shared.IntegrationMessages;

namespace OpenMind.Email.IntegrationEvents.Events;

/// <summary>
/// Event indicating email was sent successfully.
/// </summary>
public record EmailSentEvent : IntegrationEvent, IEmailEvent
{
    public Guid OrderId { get; init; }
    public string EmailType { get; init; } = string.Empty;
    public string RecipientEmail { get; init; } = string.Empty;
}
