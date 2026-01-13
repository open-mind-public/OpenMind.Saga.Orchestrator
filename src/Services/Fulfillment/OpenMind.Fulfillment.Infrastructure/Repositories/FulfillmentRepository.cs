using MongoDB.Driver;
using OpenMind.BuildingBlocks.Infrastructure.Persistence;
using OpenMind.Fulfillment.Domain.Repositories;

namespace OpenMind.Fulfillment.Infrastructure.Repositories;

public class FulfillmentRepository : MongoRepository<Domain.Aggregates.Fulfillment, Guid>, IFulfillmentRepository
{
    public FulfillmentRepository(IMongoDatabase database) : base(database, "fulfillments")
    {
    }

    public async Task<Domain.Aggregates.Fulfillment?> GetByOrderIdAsync(Guid orderId, CancellationToken cancellationToken = default)
    {
        var filter = Builders<Domain.Aggregates.Fulfillment>.Filter.Eq(x => x.OrderId, orderId);
        return await Collection.Find(filter).FirstOrDefaultAsync(cancellationToken);
    }
}
