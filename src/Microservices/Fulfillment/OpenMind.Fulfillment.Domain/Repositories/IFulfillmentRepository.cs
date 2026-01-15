using OpenMind.Shared.Domain;

namespace OpenMind.Fulfillment.Domain.Repositories;

public interface IFulfillmentRepository : IRepository<Aggregates.Fulfillment, Guid>
{
    Task<Aggregates.Fulfillment?> GetByOrderIdAsync(Guid orderId, CancellationToken cancellationToken = default);
}
