using MongoDB.Driver;
using OpenMind.BuildingBlocks.Infrastructure.Persistence;
using OpenMind.OrderPlacement.Domain.Aggregates;
using OpenMind.OrderPlacement.Domain.Enums;
using OpenMind.OrderPlacement.Domain.Repositories;

namespace OpenMind.OrderPlacement.Infrastructure.Repositories;

public class OrderRepository : MongoRepository<Order, Guid>, IOrderRepository
{
    public OrderRepository(IMongoDatabase database)
        : base(database, "orders")
    {
    }

    public async Task<IEnumerable<Order>> GetByCustomerIdAsync(
        Guid customerId,
        CancellationToken cancellationToken = default)
    {
        var filter = Builders<Order>.Filter.Eq("CustomerId.Value", customerId);
        return await Collection.Find(filter).ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Order>> GetByStatusAsync(
        string status,
        CancellationToken cancellationToken = default)
    {
        var orderStatus = OrderStatus.FromDisplayName<OrderStatus>(status);
        var filter = Builders<Order>.Filter.Eq(x => x.Status, orderStatus);
        return await Collection.Find(filter).ToListAsync(cancellationToken);
    }
}
