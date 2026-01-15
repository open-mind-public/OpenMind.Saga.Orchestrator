using OpenMind.Payment.Domain.Enums;
using OpenMind.Payment.Domain.Events;
using OpenMind.Payment.Domain.Rules;
using OpenMind.Shared.Domain;

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
        CheckRule(new PaymentAmountMustBePositiveRule(amount));
        CheckRule(new PaymentMethodMustBeProvidedRule(paymentMethod));
        CheckRule(new CardNumberMustBeValidRule(cardNumber));

        var lastFour = cardNumber.Length >= 4 ? cardNumber[^4..] : cardNumber;
        var payment = new Payment(Guid.NewGuid(), orderId, customerId, amount, paymentMethod, lastFour);
        payment.Emit(new PaymentCreatedDomainEvent(payment.Id, orderId, amount));
        return payment;
    }

    public void MarkAsProcessing(string cardNumber, string cardExpiry)
    {
        CheckRule(new PaymentMustBeInStatusRule(Status, PaymentStatus.Pending, "process"));

        Status = PaymentStatus.Processing;
        SetUpdatedAt();
        Emit(new PaymentProcessingStartedDomainEvent(Id, OrderId, Amount, cardNumber, cardExpiry));
    }

    public void MarkAsPaid(string transactionId)
    {
        CheckRule(new PaymentMustBeInStatusRule(Status, PaymentStatus.Processing, "mark as paid"));

        Status = PaymentStatus.Completed;
        TransactionId = transactionId;
        SetUpdatedAt();
        Emit(new PaymentPaidDomainEvent(Id, OrderId, Amount, transactionId));
    }

    public void MarkAsCompleted(string transactionId)
    {
        CheckRule(new PaymentMustBeInStatusRule(Status, PaymentStatus.Processing, "complete"));

        Status = PaymentStatus.Completed;
        TransactionId = transactionId;
        SetUpdatedAt();
        Emit(new PaymentCompletedDomainEvent(Id, OrderId, TransactionId));
    }

    public void MarkAsFailed(string reason)
    {
        CheckRule(new PaymentMustBeInStatusRule(Status, PaymentStatus.Processing, "fail"));

        Status = PaymentStatus.Failed;
        FailureReason = reason;
        SetUpdatedAt();
        
        Emit(new PaymentFailedDomainEvent(Id, OrderId, reason));
    }

    public void MarkAsRefunded(Guid correlationId)
    {
        CheckRule(new PaymentMustBeInStatusRule(Status, PaymentStatus.Completed, "refund"));

        Status = PaymentStatus.Refunded;
        SetUpdatedAt();
        Emit(new PaymentRefundedDomainEvent(Id, OrderId, Amount, correlationId));
    }

    public void MarkAsRefundFailed(string reason, Guid correlationId)
    {
        CheckRule(new PaymentMustBeInStatusRule(Status, PaymentStatus.Completed, "mark refund failed"));

        Status = PaymentStatus.RefundFailed;
        FailureReason = reason;
        SetUpdatedAt();
        Emit(new PaymentRefundFailedDomainEvent(Id, OrderId, reason, correlationId));
    }

    public void Retry()
    {
        CheckRule(new PaymentMustBeInStatusRule(Status, PaymentStatus.Failed, "retry"));

        Status = PaymentStatus.Processing;
        FailureReason = null;
        SetUpdatedAt();
        
        Emit(new PaymentProcessingStartedDomainEvent(Id, OrderId, Amount, "", ""));
    }
}
