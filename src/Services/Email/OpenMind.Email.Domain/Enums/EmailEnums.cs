using OpenMind.BuildingBlocks.Domain;

namespace OpenMind.Email.Domain.Enums;

public sealed class EmailStatus : Enumeration
{
    public static readonly EmailStatus Pending = new(1, nameof(Pending));
    public static readonly EmailStatus Sent = new(2, nameof(Sent));
    public static readonly EmailStatus Failed = new(3, nameof(Failed));
    public static readonly EmailStatus Delivered = new(4, nameof(Delivered));
    public static readonly EmailStatus Bounced = new(5, nameof(Bounced));

    private EmailStatus(int id, string name) : base(id, name) { }
}

public sealed class EmailType : Enumeration
{
    public static readonly EmailType OrderConfirmation = new(1, nameof(OrderConfirmation));
    public static readonly EmailType PaymentFailed = new(2, nameof(PaymentFailed));
    public static readonly EmailType OrderCancelled = new(3, nameof(OrderCancelled));
    public static readonly EmailType BackorderNotification = new(4, nameof(BackorderNotification));
    public static readonly EmailType RefundConfirmation = new(5, nameof(RefundConfirmation));
    public static readonly EmailType ShippingConfirmation = new(6, nameof(ShippingConfirmation));

    private EmailType(int id, string name) : base(id, name) { }
}
