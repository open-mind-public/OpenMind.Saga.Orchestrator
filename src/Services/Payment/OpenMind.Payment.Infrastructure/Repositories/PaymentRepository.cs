using MongoDB.Driver;
using OpenMind.BuildingBlocks.Infrastructure.Persistence;
using OpenMind.Payment.Domain.Repositories;

namespace OpenMind.Payment.Infrastructure.Repositories;

public class PaymentRepository : MongoRepository<Domain.Aggregates.Payment, Guid>, IPaymentRepository
{
    public PaymentRepository(IMongoDatabase database) : base(database, "payments")
    {
    }

    public async Task<Domain.Aggregates.Payment?> GetByOrderIdAsync(Guid orderId, CancellationToken cancellationToken = default)
    {
        var filter = Builders<Domain.Aggregates.Payment>.Filter.Eq(x => x.OrderId, orderId);
        return await Collection.Find(filter).FirstOrDefaultAsync(cancellationToken);
    }
}
