namespace OpenMind.Shared.MongoDb;

/// <summary>
/// MongoDB connection settings.
/// </summary>
public class MongoDbSettings
{
    public string ConnectionString { get; set; } = "mongodb://localhost:27019";
    public string DatabaseName { get; set; } = "OpenMindSaga";
}
