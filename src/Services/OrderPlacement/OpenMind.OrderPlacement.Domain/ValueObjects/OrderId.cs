using OpenMind.BuildingBlocks.Domain;

namespace OpenMind.OrderPlacement.Domain.ValueObjects;

/// <summary>
/// Strongly typed identifier for Order aggregate.
/// </summary>
public sealed class OrderId : StronglyTypedId<OrderId>
{
    public OrderId(Guid value) : base(value) { }

    public static OrderId Create() => new(Guid.NewGuid());
    public static OrderId From(Guid value) => new(value);
}
