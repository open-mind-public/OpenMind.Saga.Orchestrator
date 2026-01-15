using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Conventions;
using MongoDB.Bson.Serialization.Serializers;
using OpenMind.Shared.Domain;

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

#pragma warning disable CS0618 // BsonDefaults.GuidRepresentation is obsolete but needed for driver compatibility
        BsonDefaults.GuidRepresentation = GuidRepresentation.Standard;
#pragma warning restore CS0618

        // Configure Guid serialization to use standard representation globally
        BsonSerializer.RegisterSerializer(new GuidSerializer(GuidRepresentation.Standard));

        // Configure DateTime serialization
        BsonSerializer.RegisterSerializer(new DateTimeSerializer(DateTimeKind.Utc));

        // Register convention pack for consistent handling
        var pack = new ConventionPack
        {
            new CamelCaseElementNameConvention(),
            new IgnoreExtraElementsConvention(true),
            // Map "Id" property to "_id" in MongoDB only for Entity types
            new EntityIdConvention()
        };
        ConventionRegistry.Register("OpenMindConventions", pack, _ => true);

        _initialized = true;
    }
}

/// <summary>
/// Convention to map Id property to _id field in MongoDB only for Entity types.
/// This prevents nested value objects (like Enumeration) from having their Id mapped to _id.
/// </summary>
public class EntityIdConvention : ConventionBase, IMemberMapConvention
{
    public void Apply(BsonMemberMap memberMap)
    {
        // Only apply to Entity types (which have Id as their primary identifier)
        // Skip for Enumeration and other value objects
        var declaringType = memberMap.ClassMap.ClassType;
        
        if (memberMap.MemberName == "Id" && 
            !typeof(Enumeration).IsAssignableFrom(declaringType) &&
            !typeof(ValueObject).IsAssignableFrom(declaringType))
        {
            memberMap.SetElementName("_id");
        }
    }
}
