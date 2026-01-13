using OpenMind.BuildingBlocks.Domain;
using OpenMind.OrderPlacement.Domain.Aggregates;

namespace OpenMind.OrderPlacement.Domain.Repositories;

/// <summary>
/// Repository interface for Order aggregate.
/// </summary>
public interface IOrderRepository : IRepository<Order, Guid>
{
    Task<IEnumerable<Order>> GetByCustomerIdAsync(Guid customerId, CancellationToken cancellationToken = default);
    Task<IEnumerable<Order>> GetByStatusAsync(string status, CancellationToken cancellationToken = default);
}
