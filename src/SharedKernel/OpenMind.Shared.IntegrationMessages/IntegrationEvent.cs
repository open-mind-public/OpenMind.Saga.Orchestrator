namespace OpenMind.Shared.IntegrationMessages;

/// <summary>
/// Base record for integration events.
/// </summary>
public abstract record IntegrationEvent : IIntegrationEvent
{
    public Guid MessageId { get; init; } = Guid.NewGuid();
    public DateTime OccurredOn { get; init; } = DateTime.UtcNow;
    public Guid CorrelationId { get; init; }
}
