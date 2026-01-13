using MongoDB.Driver;
using OpenMind.BuildingBlocks.Domain;

namespace OpenMind.BuildingBlocks.Infrastructure.Persistence;

/// <summary>
/// Generic MongoDB repository implementation.
/// </summary>
public abstract class MongoRepository<TAggregate, TId> : IRepository<TAggregate, TId>
    where TAggregate : AggregateRoot<TId>
    where TId : notnull
{
    protected readonly IMongoCollection<TAggregate> Collection;

    protected MongoRepository(IMongoDatabase database, string collectionName)
    {
        Collection = database.GetCollection<TAggregate>(collectionName);
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
    }

    public virtual async Task UpdateAsync(TAggregate aggregate, CancellationToken cancellationToken = default)
    {
        aggregate.IncrementVersion();
        var filter = Builders<TAggregate>.Filter.Eq(x => x.Id, aggregate.Id);
        await Collection.ReplaceOneAsync(filter, aggregate, cancellationToken: cancellationToken);
    }

    public virtual async Task DeleteAsync(TId id, CancellationToken cancellationToken = default)
    {
        var filter = Builders<TAggregate>.Filter.Eq(x => x.Id, id);
        await Collection.DeleteOneAsync(filter, cancellationToken);
    }
}
