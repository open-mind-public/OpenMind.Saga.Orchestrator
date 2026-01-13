namespace OpenMind.BuildingBlocks.IntegrationEvents;

/// <summary>
/// Base interface for all integration events.
/// Integration events are used for communication between services.
/// </summary>
public interface IIntegrationEvent
{
    Guid EventId { get; }
    DateTime Timestamp { get; }
    Guid CorrelationId { get; }
}

/// <summary>
/// Base record for integration events.
/// </summary>
public abstract record IntegrationEvent : IIntegrationEvent
{
    public Guid EventId { get; init; } = Guid.NewGuid();
    public DateTime Timestamp { get; init; } = DateTime.UtcNow;
    public Guid CorrelationId { get; init; }
}
