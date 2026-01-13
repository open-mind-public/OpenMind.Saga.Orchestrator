using MediatR;
using MongoDB.Driver;
using OpenMind.BuildingBlocks.Application.DomainEvents;
using OpenMind.BuildingBlocks.Domain;

namespace OpenMind.BuildingBlocks.Infrastructure.Persistence;

/// <summary>
/// MongoDB context that tracks aggregates and dispatches domain events on save.
/// Similar to EF Core's DbContext pattern.
/// </summary>
public class MongoDbContext(IMongoClient client, IMongoDatabase database, IMediator mediator)
    : IUnitOfWork
{
    private readonly List<IAggregateRoot> _trackedAggregates = [];

    public IMongoDatabase Database => database;

    /// <summary>
    /// Tracks an aggregate for domain event dispatching.
    /// </summary>
    public void Track<TAggregate>(TAggregate aggregate) where TAggregate : class, IAggregateRoot
    {
        if (!_trackedAggregates.Contains(aggregate))
        {
            _trackedAggregates.Add(aggregate);
        }
    }

    /// <summary>
    /// Saves changes and dispatches all domain events from tracked aggregates.
    /// </summary>
    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
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

        var domainEvents = aggregatesWithEvents
            .SelectMany(a => a.DomainEvents)
            .ToList();

        // Clear events before dispatching to avoid infinite loops
        foreach (var aggregate in aggregatesWithEvents)
        {
            aggregate.ClearDomainEvents();
        }

        // Dispatch each domain event
        foreach (var domainEvent in domainEvents)
        {
            var notificationType = typeof(DomainEventNotification<>).MakeGenericType(domainEvent.GetType());
            var notification = Activator.CreateInstance(notificationType, domainEvent);

            if (notification != null)
            {
                await mediator.Publish(notification, cancellationToken);
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
