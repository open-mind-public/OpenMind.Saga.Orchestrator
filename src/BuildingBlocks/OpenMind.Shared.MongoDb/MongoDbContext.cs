using MediatR;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using OpenMind.Shared.Application.DomainEvents;
using OpenMind.Shared.Domain;

namespace OpenMind.Shared.MongoDb;

/// <summary>
/// MongoDB context that tracks aggregates and dispatches domain events on save.
/// Similar to EF Core's DbContext pattern.
/// </summary>
public class MongoDbContext(IMongoClient client, IMongoDatabase database, IMediator mediator, ILogger<MongoDbContext> logger)
    : IUnitOfWork
{
    private readonly List<IAggregateRoot> _trackedAggregates = [];
    private readonly Guid _instanceId = Guid.NewGuid();

    public IMongoDatabase Database => database;

    /// <summary>
    /// Tracks an aggregate for domain event dispatching.
    /// Uses reference equality to track instances, replacing any existing tracked instance with the same ID.
    /// </summary>
    public void Track<TAggregate>(TAggregate aggregate) where TAggregate : class, IAggregateRoot
    {
        // Use reference equality - if we have a different instance with the same ID, replace it
        // This handles the case where an aggregate is retrieved from DB, modified, and saved
        var existingIndex = -1;
        for (var i = 0; i < _trackedAggregates.Count; i++)
        {
            if (ReferenceEquals(_trackedAggregates[i], aggregate))
            {
                // Same exact instance already tracked
                logger.LogDebug("[MongoDbContext:{InstanceId}] Aggregate already tracked (same reference): {Type}", 
                    _instanceId.ToString()[..8],
                    aggregate.GetType().Name);
                return;
            }
            
            // Check if it's a different instance of the same entity (same ID)
            if (_trackedAggregates[i].Equals(aggregate))
            {
                existingIndex = i;
                break;
            }
        }

        if (existingIndex >= 0)
        {
            // Replace old instance with new one (the new one has the domain events)
            logger.LogDebug("[MongoDbContext:{InstanceId}] Replacing tracked aggregate: {Type}, OldEvents: {OldEvents}, NewEvents: {NewEvents}", 
                _instanceId.ToString()[..8],
                aggregate.GetType().Name,
                (_trackedAggregates[existingIndex] as Entity<Guid>)?.DomainEvents.Count ?? 0,
                (aggregate as Entity<Guid>)?.DomainEvents.Count ?? 0);
            _trackedAggregates[existingIndex] = aggregate;
        }
        else
        {
            _trackedAggregates.Add(aggregate);
            logger.LogDebug("[MongoDbContext:{InstanceId}] Tracked aggregate: {Type}, Events: {EventCount}, TotalTracked: {Total}", 
                _instanceId.ToString()[..8],
                aggregate.GetType().Name,
                (aggregate as Entity<Guid>)?.DomainEvents.Count ?? 0,
                _trackedAggregates.Count);
        }
    }

    /// <summary>
    /// Saves changes and dispatches all domain events from tracked aggregates.
    /// </summary>
    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        logger.LogDebug("[MongoDbContext:{InstanceId}] SaveChangesAsync - TrackedAggregates: {Count}", 
            _instanceId.ToString()[..8], 
            _trackedAggregates.Count);
        await DispatchDomainEventsAsync(cancellationToken);
        return _trackedAggregates.Count;
    }

    /// <summary>
    /// Dispatches all domain events from tracked aggregates.
    /// </summary>
    private async Task DispatchDomainEventsAsync(CancellationToken cancellationToken)
    {
        var aggregatesWithEvents = _trackedAggregates
            .OfType<Entity<Guid>>()
            .Where(a => a.DomainEvents.Count != 0)
            .ToList();

        logger.LogDebug("[MongoDbContext] AggregatesWithEvents: {Count}", aggregatesWithEvents.Count);

        var domainEvents = aggregatesWithEvents
            .SelectMany(a => a.DomainEvents)
            .ToList();

        logger.LogDebug("[MongoDbContext] DomainEvents to dispatch: {Count}", domainEvents.Count);

        // Clear events before dispatching to avoid infinite loops
        foreach (var aggregate in aggregatesWithEvents)
        {
            aggregate.ClearDomainEvents();
        }

        // Dispatch each domain event
        foreach (var domainEvent in domainEvents)
        {
            logger.LogDebug("[MongoDbContext] Dispatching domain event: {EventType}", domainEvent.GetType().Name);
            
            var notificationType = typeof(DomainEventNotification<>).MakeGenericType(domainEvent.GetType());
            var notification = Activator.CreateInstance(notificationType, domainEvent);

            if (notification != null)
            {
                await mediator.Publish(notification, cancellationToken);
                logger.LogDebug("[MongoDbContext] Dispatched domain event: {EventType}", domainEvent.GetType().Name);
            }
        }
    }

    /// <summary>
    /// Starts a MongoDB session for transactions.
    /// </summary>
    public async Task<IClientSessionHandle> StartSessionAsync(CancellationToken cancellationToken = default)
    {
        return await client.StartSessionAsync(cancellationToken: cancellationToken);
    }

    /// <summary>
    /// Clears all tracked aggregates.
    /// </summary>
    public void ClearTracking()
    {
        _trackedAggregates.Clear();
    }
}
