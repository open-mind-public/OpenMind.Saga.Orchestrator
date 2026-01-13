namespace OpenMind.BuildingBlocks.IntegrationEvents;

/// <summary>
/// Base interface for all integration messages (commands and events).
/// </summary>
public interface IMessage
{
    Guid MessageId { get; }
    DateTime OccurredOn { get; }
    Guid CorrelationId { get; }
}

/// <summary>
/// Marker interface for integration commands.
/// Commands represent requests for an action to be performed.
/// </summary>
public interface IIntegrationCommand : IMessage { }

/// <summary>
/// Marker interface for integration events.
/// Events represent notifications that something has happened.
/// </summary>
public interface IIntegrationEvent : IMessage { }

/// <summary>
/// Base record for integration commands.
/// </summary>
public abstract record IntegrationCommand : IIntegrationCommand
{
    public Guid MessageId { get; init; } = Guid.NewGuid();
    public DateTime OccurredOn { get; init; } = DateTime.UtcNow;
    public Guid CorrelationId { get; init; }
}

/// <summary>
/// Base record for integration events.
/// </summary>
public abstract record IntegrationEvent : IIntegrationEvent
{
    public Guid MessageId { get; init; } = Guid.NewGuid();
    public DateTime OccurredOn { get; init; } = DateTime.UtcNow;
    public Guid CorrelationId { get; init; }
}
