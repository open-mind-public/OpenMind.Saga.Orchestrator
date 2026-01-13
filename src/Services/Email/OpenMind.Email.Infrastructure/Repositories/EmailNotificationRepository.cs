using MongoDB.Driver;
using OpenMind.Email.Domain.Repositories;
using OpenMind.Shared.MongoDb;

namespace OpenMind.Email.Infrastructure.Repositories;

public class EmailNotificationRepository : MongoRepository<Domain.Aggregates.EmailNotification, Guid>, IEmailNotificationRepository
{
    public EmailNotificationRepository(IMongoDatabase database) : base(database, "email_notifications")
    {
    }

    public async Task<IEnumerable<Domain.Aggregates.EmailNotification>> GetByOrderIdAsync(
        Guid orderId,
        CancellationToken cancellationToken = default)
    {
        var filter = Builders<Domain.Aggregates.EmailNotification>.Filter.Eq(x => x.OrderId, orderId);
        return await Collection.Find(filter).ToListAsync(cancellationToken);
    }
}
