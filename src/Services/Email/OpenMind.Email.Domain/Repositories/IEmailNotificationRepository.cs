using OpenMind.Shared.Domain;

namespace OpenMind.Email.Domain.Repositories;

public interface IEmailNotificationRepository : IRepository<Aggregates.EmailNotification, Guid>
{
    Task<IEnumerable<Aggregates.EmailNotification>> GetByOrderIdAsync(Guid orderId, CancellationToken cancellationToken = default);
}
