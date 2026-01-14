using OpenMind.Shared.Domain;

namespace OpenMind.Order.Domain.Enums;

/// <summary>
/// Smart enum representing order statuses.
/// </summary>
public sealed class OrderStatus : Enumeration
{
    public static readonly OrderStatus Pending = new(1, nameof(Pending));
    public static readonly OrderStatus PaymentProcessing = new(2, nameof(PaymentProcessing));
    public static readonly OrderStatus PaymentCompleted = new(3, nameof(PaymentCompleted));
    public static readonly OrderStatus PaymentFailed = new(4, nameof(PaymentFailed));
    public static readonly OrderStatus Fulfilling = new(5, nameof(Fulfilling));
    public static readonly OrderStatus Shipped = new(6, nameof(Shipped));
    public static readonly OrderStatus Delivered = new(7, nameof(Delivered));
    public static readonly OrderStatus Cancelled = new(8, nameof(Cancelled));
    public static readonly OrderStatus Refunded = new(9, nameof(Refunded));
    public static readonly OrderStatus BackOrdered = new(10, nameof(BackOrdered));

    // Required for MongoDB deserialization
    private OrderStatus() : base() { }

    private OrderStatus(int id, string name) : base(id, name) { }
}
