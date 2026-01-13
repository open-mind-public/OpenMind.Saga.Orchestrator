using OpenMind.Shared.Domain;

namespace OpenMind.Fulfillment.Domain.Enums;

public sealed class FulfillmentStatus : Enumeration
{
    public static readonly FulfillmentStatus Pending = new(1, nameof(Pending));
    public static readonly FulfillmentStatus Processing = new(2, nameof(Processing));
    public static readonly FulfillmentStatus Shipped = new(3, nameof(Shipped));
    public static readonly FulfillmentStatus Delivered = new(4, nameof(Delivered));
    public static readonly FulfillmentStatus BackOrdered = new(5, nameof(BackOrdered));
    public static readonly FulfillmentStatus Cancelled = new(6, nameof(Cancelled));
    public static readonly FulfillmentStatus Failed = new(7, nameof(Failed));

    private FulfillmentStatus(int id, string name) : base(id, name) { }
}
