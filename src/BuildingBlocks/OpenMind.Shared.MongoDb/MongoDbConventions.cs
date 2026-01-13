using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;

namespace OpenMind.Shared.MongoDb;

/// <summary>
/// MongoDB serialization conventions and configurations.
/// </summary>
public static class MongoDbConventions
{
    private static bool _initialized;

    public static void Initialize()
    {
        if (_initialized) return;

        // Configure Guid serialization to use standard representation
        BsonSerializer.RegisterSerializer(new GuidSerializer(GuidRepresentation.Standard));

        // Configure DateTime serialization
        BsonSerializer.RegisterSerializer(new DateTimeSerializer(DateTimeKind.Utc));

        _initialized = true;
    }
}
