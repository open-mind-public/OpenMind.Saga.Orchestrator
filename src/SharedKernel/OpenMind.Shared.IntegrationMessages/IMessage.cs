namespace OpenMind.Shared.IntegrationMessages;

/// <summary>
/// Base interface for all integration messages (commands and events).
/// </summary>
public interface IMessage
{
    Guid MessageId { get; }
    DateTime OccurredOn { get; }
    Guid CorrelationId { get; }
}
