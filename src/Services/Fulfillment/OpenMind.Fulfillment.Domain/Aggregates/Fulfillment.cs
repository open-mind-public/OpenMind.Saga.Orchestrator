using OpenMind.BuildingBlocks.Domain;
using OpenMind.Fulfillment.Domain.Enums;
using OpenMind.Fulfillment.Domain.Events;

namespace OpenMind.Fulfillment.Domain.Aggregates;

public class Fulfillment : AggregateRoot<Guid>
{
    private readonly List<FulfillmentItem> _items = [];

    public Guid OrderId { get; private set; }
    public Guid CustomerId { get; private set; }
    public string ShippingAddress { get; private set; }
    public FulfillmentStatus Status { get; private set; }
    public string? TrackingNumber { get; private set; }
    public DateTime? EstimatedDelivery { get; private set; }
    public string? FailureReason { get; private set; }
    public IReadOnlyCollection<FulfillmentItem> Items => _items.AsReadOnly();

    private Fulfillment() : base()
    {
        ShippingAddress = string.Empty;
        Status = FulfillmentStatus.Pending;
    }

    private Fulfillment(Guid id, Guid orderId, Guid customerId, string shippingAddress)
        : base(id)
    {
        OrderId = orderId;
        CustomerId = customerId;
        ShippingAddress = shippingAddress;
        Status = FulfillmentStatus.Pending;
    }

    public static Fulfillment Create(Guid orderId, Guid customerId, string shippingAddress)
    {
        var fulfillment = new Fulfillment(Guid.NewGuid(), orderId, customerId, shippingAddress);
        fulfillment.AddDomainEvent(new FulfillmentCreatedDomainEvent(fulfillment.Id, orderId));
        return fulfillment;
    }

    public void AddItem(Guid productId, string productName, int quantity)
    {
        _items.Add(new FulfillmentItem(productId, productName, quantity));
    }

    public void MarkAsProcessing()
    {
        if (Status != FulfillmentStatus.Pending)
            throw new InvalidOperationException($"Cannot process fulfillment in {Status} status");

        Status = FulfillmentStatus.Processing;
        SetUpdatedAt();
    }

    public void MarkAsShipped(string trackingNumber)
    {
        if (Status != FulfillmentStatus.Processing)
            throw new InvalidOperationException($"Cannot ship fulfillment in {Status} status");

        Status = FulfillmentStatus.Shipped;
        TrackingNumber = trackingNumber;
        EstimatedDelivery = DateTime.UtcNow.AddDays(Random.Shared.Next(3, 7));
        SetUpdatedAt();
        AddDomainEvent(new FulfillmentShippedDomainEvent(Id, OrderId, TrackingNumber));
    }

    public void MarkAsBackOrdered(string reason)
    {
        if (Status != FulfillmentStatus.Processing)
            throw new InvalidOperationException($"Cannot backorder fulfillment in {Status} status");

        Status = FulfillmentStatus.BackOrdered;
        FailureReason = reason;
        SetUpdatedAt();
        AddDomainEvent(new FulfillmentBackOrderedDomainEvent(Id, OrderId, reason));
    }

    public void MarkAsFailed(string reason)
    {
        Status = FulfillmentStatus.Failed;
        FailureReason = reason;
        SetUpdatedAt();
    }

    public void Cancel()
    {
        var allowedStatuses = new[] { FulfillmentStatus.Pending, FulfillmentStatus.Processing, FulfillmentStatus.BackOrdered };
        if (!allowedStatuses.Contains(Status))
            throw new InvalidOperationException($"Cannot cancel fulfillment in {Status} status");

        Status = FulfillmentStatus.Cancelled;
        SetUpdatedAt();
        AddDomainEvent(new FulfillmentCancelledDomainEvent(Id, OrderId));
    }
}

public record FulfillmentItem(Guid ProductId, string ProductName, int Quantity);
