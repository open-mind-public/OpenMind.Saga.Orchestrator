namespace OpenMind.Shared.Domain;

/// <summary>
/// Unit of work interface for coordinating persistence operations.
/// </summary>
public interface IUnitOfWork
{
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
