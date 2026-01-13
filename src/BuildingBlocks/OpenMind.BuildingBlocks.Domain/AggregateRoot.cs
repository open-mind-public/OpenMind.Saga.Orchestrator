namespace OpenMind.BuildingBlocks.Domain;

/// <summary>
/// Base class for aggregate roots following DDD tactical patterns.
/// An aggregate root is the entry point to an aggregate cluster of domain objects.
/// </summary>
public abstract class AggregateRoot<TId> : Entity<TId>, IAggregateRoot
    where TId : notnull
{
    public int Version { get; protected set; }

    protected AggregateRoot() : base() { }

    protected AggregateRoot(TId id) : base(id) { }

    public void IncrementVersion()
    {
        Version++;
        SetUpdatedAt();
    }
}

/// <summary>
/// Marker interface for aggregate roots
/// </summary>
public interface IAggregateRoot
{
    int Version { get; }
}
