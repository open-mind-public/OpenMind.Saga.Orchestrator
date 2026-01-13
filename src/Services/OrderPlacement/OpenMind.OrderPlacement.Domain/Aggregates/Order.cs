using OpenMind.BuildingBlocks.Domain;
using OpenMind.OrderPlacement.Domain.Entities;
using OpenMind.OrderPlacement.Domain.Enums;
using OpenMind.OrderPlacement.Domain.Events;
using OpenMind.OrderPlacement.Domain.ValueObjects;

namespace OpenMind.OrderPlacement.Domain.Aggregates;

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
        order.AddDomainEvent(new OrderCreatedDomainEvent(orderId, customerId.Value));
        return order;
    }

    public void AddItem(OrderItem item)
    {
        _items.Add(item);
        RecalculateTotal();
        AddDomainEvent(new OrderItemAddedDomainEvent(Id, item.ProductId, item.Quantity));
    }

    public void SetPaymentProcessing()
    {
        if (Status != OrderStatus.Pending)
            throw new InvalidOperationException($"Cannot transition from {Status} to PaymentProcessing");

        Status = OrderStatus.PaymentProcessing;
        SetUpdatedAt();
    }

    public void SetPaymentCompleted(string transactionId)
    {
        if (Status != OrderStatus.PaymentProcessing)
            throw new InvalidOperationException($"Cannot transition from {Status} to PaymentCompleted");

        Status = OrderStatus.PaymentCompleted;
        PaymentTransactionId = transactionId;
        SetUpdatedAt();
        AddDomainEvent(new OrderPaymentCompletedDomainEvent(Id, transactionId));
    }

    public void SetPaymentFailed(string reason)
    {
        if (Status != OrderStatus.PaymentProcessing)
            throw new InvalidOperationException($"Cannot transition from {Status} to PaymentFailed");

        Status = OrderStatus.PaymentFailed;
        CancellationReason = reason;
        SetUpdatedAt();
        AddDomainEvent(new OrderPaymentFailedDomainEvent(Id, reason));
    }

    public void SetFulfilling()
    {
        if (Status != OrderStatus.PaymentCompleted)
            throw new InvalidOperationException($"Cannot transition from {Status} to Fulfilling");

        Status = OrderStatus.Fulfilling;
        SetUpdatedAt();
    }

    public void SetShipped(string trackingNumber)
    {
        if (Status != OrderStatus.Fulfilling)
            throw new InvalidOperationException($"Cannot transition from {Status} to Shipped");

        Status = OrderStatus.Shipped;
        TrackingNumber = trackingNumber;
        SetUpdatedAt();
        AddDomainEvent(new OrderShippedDomainEvent(Id, trackingNumber));
    }

    public void SetBackOrdered(string reason)
    {
        if (Status != OrderStatus.Fulfilling)
            throw new InvalidOperationException($"Cannot transition from {Status} to BackOrdered");

        Status = OrderStatus.BackOrdered;
        CancellationReason = reason;
        SetUpdatedAt();
        AddDomainEvent(new OrderBackOrderedDomainEvent(Id, reason));
    }

    public void Cancel(string reason)
    {
        var allowedStatuses = new[] { OrderStatus.Pending, OrderStatus.PaymentFailed, OrderStatus.BackOrdered };
        if (!allowedStatuses.Contains(Status))
            throw new InvalidOperationException($"Cannot cancel order in {Status} status");

        Status = OrderStatus.Cancelled;
        CancellationReason = reason;
        SetUpdatedAt();
        AddDomainEvent(new OrderCancelledDomainEvent(Id, reason));
    }

    public void SetRefunded()
    {
        var allowedStatuses = new[] { OrderStatus.BackOrdered, OrderStatus.Cancelled };
        if (!allowedStatuses.Contains(Status))
            throw new InvalidOperationException($"Cannot refund order in {Status} status");

        Status = OrderStatus.Refunded;
        SetUpdatedAt();
        AddDomainEvent(new OrderRefundedDomainEvent(Id));
    }

    public void UpdateStatus(string status, string? reason = null)
    {
        Status = Enumeration.FromDisplayName<OrderStatus>(status);
        if (!string.IsNullOrEmpty(reason))
            CancellationReason = reason;
        SetUpdatedAt();
    }

    private void RecalculateTotal()
    {
        TotalAmount = _items.Aggregate(Money.Zero(), (total, item) => total.Add(item.TotalPrice));
    }
}
