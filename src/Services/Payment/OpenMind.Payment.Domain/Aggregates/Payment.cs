using OpenMind.BuildingBlocks.Domain;
using OpenMind.Payment.Domain.Enums;
using OpenMind.Payment.Domain.Events;

namespace OpenMind.Payment.Domain.Aggregates;

/// <summary>
/// Payment aggregate root.
/// </summary>
public class Payment : AggregateRoot<Guid>
{
    public Guid OrderId { get; private set; }
    public Guid CustomerId { get; private set; }
    public decimal Amount { get; private set; }
    public string PaymentMethod { get; private set; }
    public PaymentStatus Status { get; private set; }
    public string? TransactionId { get; private set; }
    public string? FailureReason { get; private set; }
    public string? CardLastFourDigits { get; private set; }

    private Payment() : base()
    {
        PaymentMethod = string.Empty;
        Status = PaymentStatus.Pending;
    }

    private Payment(Guid id, Guid orderId, Guid customerId, decimal amount, string paymentMethod, string cardLastFour)
        : base(id)
    {
        OrderId = orderId;
        CustomerId = customerId;
        Amount = amount;
        PaymentMethod = paymentMethod;
        CardLastFourDigits = cardLastFour;
        Status = PaymentStatus.Pending;
    }

    public static Payment Create(Guid orderId, Guid customerId, decimal amount, string paymentMethod, string cardNumber)
    {
        var lastFour = cardNumber.Length >= 4 ? cardNumber[^4..] : cardNumber;
        var payment = new Payment(Guid.NewGuid(), orderId, customerId, amount, paymentMethod, lastFour);
        payment.AddDomainEvent(new PaymentCreatedDomainEvent(payment.Id, orderId, amount));
        return payment;
    }

    public void MarkAsProcessing()
    {
        if (Status != PaymentStatus.Pending)
            throw new InvalidOperationException($"Cannot process payment in {Status} status");

        Status = PaymentStatus.Processing;
        SetUpdatedAt();
    }

    public void MarkAsCompleted(string transactionId)
    {
        if (Status != PaymentStatus.Processing)
            throw new InvalidOperationException($"Cannot complete payment in {Status} status");

        Status = PaymentStatus.Completed;
        TransactionId = transactionId;
        SetUpdatedAt();
        AddDomainEvent(new PaymentCompletedDomainEvent(Id, OrderId, TransactionId));
    }

    public void MarkAsFailed(string reason)
    {
        if (Status != PaymentStatus.Processing)
            throw new InvalidOperationException($"Cannot fail payment in {Status} status");

        Status = PaymentStatus.Failed;
        FailureReason = reason;
        SetUpdatedAt();
        AddDomainEvent(new PaymentFailedDomainEvent(Id, OrderId, reason));
    }

    public void MarkAsRefunded()
    {
        if (Status != PaymentStatus.Completed)
            throw new InvalidOperationException($"Cannot refund payment in {Status} status");

        Status = PaymentStatus.Refunded;
        SetUpdatedAt();
        AddDomainEvent(new PaymentRefundedDomainEvent(Id, OrderId, Amount));
    }

    public void MarkAsRefundFailed(string reason)
    {
        if (Status != PaymentStatus.Completed)
            throw new InvalidOperationException($"Cannot mark refund failed in {Status} status");

        Status = PaymentStatus.RefundFailed;
        FailureReason = reason;
        SetUpdatedAt();
    }
}
