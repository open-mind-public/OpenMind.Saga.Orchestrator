using OpenMind.Shared.Domain;

namespace OpenMind.Payment.Domain.Repositories;

public interface IPaymentRepository : IRepository<Aggregates.Payment, Guid>
{
    Task<Aggregates.Payment?> GetByOrderIdAsync(Guid orderId, CancellationToken cancellationToken = default);
}
