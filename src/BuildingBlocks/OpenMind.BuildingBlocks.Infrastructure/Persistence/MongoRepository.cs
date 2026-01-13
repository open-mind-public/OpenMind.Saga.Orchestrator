using MongoDB.Driver;
using OpenMind.BuildingBlocks.Domain;

namespace OpenMind.BuildingBlocks.Infrastructure.Persistence;

/// <summary>
/// Generic MongoDB repository implementation.
/// Automatically tracks aggregates in MongoDbContext for domain event dispatching.
/// </summary>
public abstract class MongoRepository<TAggregate, TId> : IRepository<TAggregate, TId>
    where TAggregate : AggregateRoot<TId>
    where TId : notnull
{
    protected readonly IMongoCollection<TAggregate> Collection;
    protected readonly MongoDbContext? DbContext;

    protected MongoRepository(IMongoDatabase database, string collectionName)
    {
        Collection = database.GetCollection<TAggregate>(collectionName);
    }

    protected MongoRepository(MongoDbContext dbContext, string collectionName)
    {
        DbContext = dbContext;
        Collection = dbContext.Database.GetCollection<TAggregate>(collectionName);
    }

    public virtual async Task<TAggregate?> GetByIdAsync(TId id, CancellationToken cancellationToken = default)
    {
        var filter = Builders<TAggregate>.Filter.Eq(x => x.Id, id);
        return await Collection.Find(filter).FirstOrDefaultAsync(cancellationToken);
    }

    public virtual async Task<IEnumerable<TAggregate>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await Collection.Find(_ => true).ToListAsync(cancellationToken);
    }

    public virtual async Task AddAsync(TAggregate aggregate, CancellationToken cancellationToken = default)
    {
        await Collection.InsertOneAsync(aggregate, cancellationToken: cancellationToken);
        DbContext?.Track(aggregate);
    }

    public virtual async Task UpdateAsync(TAggregate aggregate, CancellationToken cancellationToken = default)
    {
        aggregate.IncrementVersion();
        var filter = Builders<TAggregate>.Filter.Eq(x => x.Id, aggregate.Id);
        await Collection.ReplaceOneAsync(filter, aggregate, cancellationToken: cancellationToken);
        DbContext?.Track(aggregate);
    }

    public virtual async Task DeleteAsync(TId id, CancellationToken cancellationToken = default)
    {
        var filter = Builders<TAggregate>.Filter.Eq(x => x.Id, id);
        await Collection.DeleteOneAsync(filter, cancellationToken);
    }
}
