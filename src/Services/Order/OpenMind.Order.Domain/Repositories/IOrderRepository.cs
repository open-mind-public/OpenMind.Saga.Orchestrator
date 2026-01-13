using OpenMind.BuildingBlocks.Domain;
using OpenMind.Order.Domain.Aggregates;

namespace OpenMind.Order.Domain.Repositories;

// Use alias to avoid namespace conflict with Order class
using OrderAggregate = Aggregates.Order;

/// <summary>
/// Repository interface for Order aggregate.
/// </summary>
public interface IOrderRepository : IRepository<OrderAggregate, Guid>
{
    Task<IEnumerable<OrderAggregate>> GetByCustomerIdAsync(Guid customerId, CancellationToken cancellationToken = default);
    Task<IEnumerable<OrderAggregate>> GetByStatusAsync(string status, CancellationToken cancellationToken = default);
}
