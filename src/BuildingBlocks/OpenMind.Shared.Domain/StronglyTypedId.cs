namespace OpenMind.Shared.Domain;

/// <summary>
/// Base class for strongly typed identifiers.
/// Provides type safety for entity identifiers and prevents mixing IDs of different entity types.
/// </summary>
public abstract class StronglyTypedId<T> : ValueObject
    where T : StronglyTypedId<T>
{
    public Guid Value { get; protected set; }

    // Required for MongoDB deserialization
    protected StronglyTypedId()
    {
        Value = Guid.Empty;
    }

    protected StronglyTypedId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("Id cannot be empty", nameof(value));

        Value = value;
    }

    public static implicit operator Guid(StronglyTypedId<T> id) => id.Value;

    public override string ToString() => Value.ToString();

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value;
    }
}
