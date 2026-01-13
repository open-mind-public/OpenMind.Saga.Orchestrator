using MongoDB.Driver;
using OpenMind.BuildingBlocks.Domain;

namespace OpenMind.BuildingBlocks.Infrastructure.Persistence;

/// <summary>
/// MongoDB implementation of the Unit of Work pattern.
/// </summary>
public class MongoUnitOfWork : IUnitOfWork
{
    private readonly IMongoClient _client;

    public MongoUnitOfWork(IMongoClient client)
    {
        _client = client;
    }

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        // MongoDB doesn't have traditional transaction support like SQL databases
        // For simple cases, we can use the session to commit changes
        // In a real-world scenario, you might use MongoDB transactions
        await Task.CompletedTask;
        return 1;
    }

    public async Task<IClientSessionHandle> StartSessionAsync(CancellationToken cancellationToken = default)
    {
        return await _client.StartSessionAsync(cancellationToken: cancellationToken);
    }
}
