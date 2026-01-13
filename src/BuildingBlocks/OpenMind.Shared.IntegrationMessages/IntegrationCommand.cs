namespace OpenMind.Shared.IntegrationMessages;

/// <summary>
/// Base record for integration commands.
/// </summary>
public abstract record IntegrationCommand : IIntegrationCommand
{
    public Guid MessageId { get; init; } = Guid.NewGuid();
    public DateTime OccurredOn { get; init; } = DateTime.UtcNow;
    public Guid CorrelationId { get; init; }
}
