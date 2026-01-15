using MongoDB.Driver;
using OpenMind.Order.Domain.Aggregates;
using OpenMind.Order.Domain.Enums;
using OpenMind.Order.Domain.Repositories;
using OpenMind.Shared.MongoDb;
using OrderAggregate = OpenMind.Order.Domain.Aggregates.Order;

namespace OpenMind.Order.Infrastructure.Repositories;

public class OrderRepository : MongoRepository<OrderAggregate, Guid>, IOrderRepository
{
    public OrderRepository(MongoDbContext dbContext)
        : base(dbContext, "orders")
    {
    }

    public async Task<IEnumerable<OrderAggregate>> GetByCustomerIdAsync(
        Guid customerId,
        CancellationToken cancellationToken = default)
    {
        var filter = Builders<OrderAggregate>.Filter.Eq("CustomerId.Value", customerId);
        return await Collection.Find(filter).ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<OrderAggregate>> GetByStatusAsync(
        string status,
        CancellationToken cancellationToken = default)
    {
        var orderStatus = OrderStatus.FromDisplayName<OrderStatus>(status);
        var filter = Builders<OrderAggregate>.Filter.Eq(x => x.Status, orderStatus);
        return await Collection.Find(filter).ToListAsync(cancellationToken);
    }
}
