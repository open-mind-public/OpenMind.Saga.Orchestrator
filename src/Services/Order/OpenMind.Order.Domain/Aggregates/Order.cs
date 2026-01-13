using OpenMind.Order.Domain.Entities;
using OpenMind.Order.Domain.Enums;
using OpenMind.Order.Domain.Events;
using OpenMind.Order.Domain.Rules;
using OpenMind.Order.Domain.ValueObjects;
using OpenMind.Shared.Domain;

namespace OpenMind.Order.Domain.Aggregates;

/// <summary>
/// Order aggregate root following DDD tactical patterns.
/// </summary>
public class Order : AggregateRoot<Guid>
{
    private readonly List<OrderItem> _items = [];

    public CustomerId CustomerId { get; private set; }
    public OrderStatus Status { get; private set; }
    public Address ShippingAddress { get; private set; }
    public Money TotalAmount { get; private set; }
    public string? CancellationReason { get; private set; }
    public string? PaymentTransactionId { get; private set; }
    public string? TrackingNumber { get; private set; }
    public IReadOnlyCollection<OrderItem> Items => _items.AsReadOnly();

    private Order() : base()
    {
        CustomerId = CustomerId.Create();
        Status = OrderStatus.Pending;
        ShippingAddress = Address.Create("Default", "Default", "Default", "00000", "Default");
        TotalAmount = Money.Zero();
    }

    private Order(Guid id, CustomerId customerId, Address shippingAddress)
        : base(id)
    {
        CustomerId = customerId;
        ShippingAddress = shippingAddress;
        Status = OrderStatus.Pending;
        TotalAmount = Money.Zero();
    }

    public static Order Create(Guid orderId, CustomerId customerId, Address shippingAddress)
    {
        var order = new Order(orderId, customerId, shippingAddress);
        order.Emit(new OrderCreatedDomainEvent(orderId, customerId.Value));
        return order;
    }

    public void AddItem(OrderItem item)
    {
        _items.Add(item);
        RecalculateTotal();
        Emit(new OrderItemAddedDomainEvent(Id, item.ProductId, item.Quantity));
    }

    public void SetPaymentProcessing()
    {
        CheckRule(new OrderMustBeInStatusRule(Status, OrderStatus.Pending, "transition to PaymentProcessing"));

        Status = OrderStatus.PaymentProcessing;
        SetUpdatedAt();
    }

    public void SetPaymentCompleted(string transactionId, Guid correlationId)
    {
        CheckRule(new OrderMustBeInStatusRule(Status, OrderStatus.PaymentProcessing, "transition to PaymentCompleted"));

        Status = OrderStatus.PaymentCompleted;
        PaymentTransactionId = transactionId;
        SetUpdatedAt();
        
        Emit(new OrderPaymentCompletedDomainEvent(Id, transactionId, correlationId));
    }

    public void SetPaymentFailed(string reason, Guid correlationId)
    {
        CheckRule(new OrderMustBeInStatusRule(Status, OrderStatus.PaymentProcessing, "transition to PaymentFailed"));

        Status = OrderStatus.PaymentFailed;
        CancellationReason = reason;
        SetUpdatedAt();
        
        Emit(new OrderPaymentFailedDomainEvent(Id, reason, correlationId));
    }

    public void SetFulfilling()
    {
        CheckRule(new OrderMustBeInStatusRule(Status, OrderStatus.PaymentCompleted, "transition to Fulfilling"));

        Status = OrderStatus.Fulfilling;
        SetUpdatedAt();
    }

    public void SetShipped(string trackingNumber, Guid correlationId)
    {
        CheckRule(new OrderMustBeInStatusRule(Status, OrderStatus.Fulfilling, "transition to Shipped"));

        Status = OrderStatus.Shipped;
        TrackingNumber = trackingNumber;
        SetUpdatedAt();
        Emit(new OrderShippedDomainEvent(Id, trackingNumber, correlationId));
    }

    public void SetBackOrdered(string reason, Guid correlationId)
    {
        CheckRule(new OrderMustBeInStatusRule(Status, OrderStatus.Fulfilling, "transition to BackOrdered"));

        Status = OrderStatus.BackOrdered;
        CancellationReason = reason;
        SetUpdatedAt();
        Emit(new OrderBackOrderedDomainEvent(Id, reason, correlationId));
    }

    public void Cancel(string reason, Guid correlationId)
    {
        var allowedStatuses = new[] { OrderStatus.Pending, OrderStatus.PaymentFailed, OrderStatus.BackOrdered };
        CheckRule(new OrderMustBeInOneOfStatusesRule(Status, allowedStatuses, "cancel"));

        Status = OrderStatus.Cancelled;
        CancellationReason = reason;
        SetUpdatedAt();
        
        Emit(new OrderCancelledDomainEvent(Id, reason, correlationId));
    }

    public void SetRefunded()
    {
        var allowedStatuses = new[] { OrderStatus.BackOrdered, OrderStatus.Cancelled };
        CheckRule(new OrderMustBeInOneOfStatusesRule(Status, allowedStatuses, "refund"));

        Status = OrderStatus.Refunded;
        SetUpdatedAt();
        Emit(new OrderRefundedDomainEvent(Id));
    }

    private void RecalculateTotal()
    {
        TotalAmount = _items.Aggregate(Money.Zero(), (total, item) => total.Add(item.TotalPrice));
    }
}
