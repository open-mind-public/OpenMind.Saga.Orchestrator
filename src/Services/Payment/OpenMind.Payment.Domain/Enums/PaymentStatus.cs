using OpenMind.Shared.Domain;

namespace OpenMind.Payment.Domain.Enums;

public sealed class PaymentStatus : Enumeration
{
    public static readonly PaymentStatus Pending = new(1, nameof(Pending));
    public static readonly PaymentStatus Processing = new(2, nameof(Processing));
    public static readonly PaymentStatus Completed = new(3, nameof(Completed));
    public static readonly PaymentStatus Failed = new(4, nameof(Failed));
    public static readonly PaymentStatus Refunded = new(5, nameof(Refunded));
    public static readonly PaymentStatus RefundFailed = new(6, nameof(RefundFailed));

    private PaymentStatus(int id, string name) : base(id, name) { }
}
