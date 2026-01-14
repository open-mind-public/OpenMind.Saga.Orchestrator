using OpenMind.Fulfillment.Domain.Enums;
using OpenMind.Fulfillment.Domain.Events;
using OpenMind.Fulfillment.Domain.Rules;
using OpenMind.Shared.Domain;

namespace OpenMind.Fulfillment.Domain.Aggregates;

public class Fulfillment : AggregateRoot<Guid>
{
    // Use a property with private setter for MongoDB serialization
    public List<FulfillmentItem> Items { get; private set; } = [];

    public Guid OrderId { get; private set; }
    public Guid CustomerId { get; private set; }
    public string ShippingAddress { get; private set; }
    public FulfillmentStatus Status { get; private set; }
    public string? TrackingNumber { get; private set; }
    public DateTime? EstimatedDelivery { get; private set; }
    public string? FailureReason { get; private set; }

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
        CheckRule(new FulfillmentShippingAddressMustBeProvidedRule(shippingAddress));

        var fulfillment = new Fulfillment(Guid.NewGuid(), orderId, customerId, shippingAddress);
        fulfillment.Emit(new FulfillmentCreatedDomainEvent(fulfillment.Id, orderId));
        return fulfillment;
    }

    public void AddItem(Guid productId, string productName, int quantity)
    {
        Items.Add(new FulfillmentItem(productId, productName, quantity));
    }

    public void MarkAsProcessing()
    {
        CheckRule(new FulfillmentMustBeInStatusRule(Status, FulfillmentStatus.Pending, "process"));

        Status = FulfillmentStatus.Processing;
        SetUpdatedAt();
    }

    public void MarkAsShipped(string trackingNumber, Guid correlationId)
    {
        CheckRule(new FulfillmentMustBeInStatusRule(Status, FulfillmentStatus.Processing, "ship"));
        CheckRule(new TrackingNumberMustBeProvidedRule(trackingNumber));

        Status = FulfillmentStatus.Shipped;
        TrackingNumber = trackingNumber;
        EstimatedDelivery = DateTime.UtcNow.AddDays(Random.Shared.Next(3, 7));
        SetUpdatedAt();
        Emit(new FulfillmentShippedDomainEvent(Id, OrderId, TrackingNumber, EstimatedDelivery.Value, correlationId));
    }

    public void MarkAsBackOrdered(string reason, Guid correlationId)
    {
        CheckRule(new FulfillmentMustBeInStatusRule(Status, FulfillmentStatus.Processing, "backorder"));

        Status = FulfillmentStatus.BackOrdered;
        FailureReason = reason;
        SetUpdatedAt();
        Emit(new FulfillmentBackOrderedDomainEvent(Id, OrderId, reason, correlationId));
    }

    public void MarkAsFailed(string reason)
    {
        Status = FulfillmentStatus.Failed;
        FailureReason = reason;
        SetUpdatedAt();
    }

    public void Cancel(Guid correlationId)
    {
        var allowedStatuses = new[] { FulfillmentStatus.Pending, FulfillmentStatus.Processing, FulfillmentStatus.BackOrdered };
        CheckRule(new FulfillmentMustBeInOneOfStatusesRule(Status, allowedStatuses, "cancel"));

        Status = FulfillmentStatus.Cancelled;
        SetUpdatedAt();
        Emit(new FulfillmentCancelledDomainEvent(Id, OrderId, correlationId));
    }
}

/// <summary>
/// Represents an item in a fulfillment (Value Object).
/// </summary>
public class FulfillmentItem : ValueObject
{
    public Guid ProductId { get; private set; }
    public string ProductName { get; private set; }
    public int Quantity { get; private set; }

    // Parameterless constructor for MongoDB deserialization
    private FulfillmentItem()
    {
        ProductName = string.Empty;
    }

    public FulfillmentItem(Guid productId, string productName, int quantity)
    {
        ProductId = productId;
        ProductName = productName;
        Quantity = quantity;
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return ProductId;
        yield return ProductName;
        yield return Quantity;
    }
}
