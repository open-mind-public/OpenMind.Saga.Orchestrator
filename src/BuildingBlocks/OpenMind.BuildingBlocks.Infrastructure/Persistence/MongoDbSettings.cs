namespace OpenMind.BuildingBlocks.Infrastructure.Persistence;

/// <summary>
/// MongoDB connection settings.
/// </summary>
public class MongoDbSettings
{
    public string ConnectionString { get; set; } = "mongodb://localhost:27017";
    public string DatabaseName { get; set; } = "OpenMindSaga";
}
