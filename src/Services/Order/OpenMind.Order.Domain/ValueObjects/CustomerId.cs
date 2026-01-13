using OpenMind.Shared.Domain;

namespace OpenMind.Order.Domain.ValueObjects;

/// <summary>
/// Strongly typed identifier for Customer.
/// </summary>
public sealed class CustomerId : StronglyTypedId<CustomerId>
{
    public CustomerId(Guid value) : base(value) { }

    public static CustomerId Create() => new(Guid.NewGuid());
    public static CustomerId From(Guid value) => new(value);
}
