using OpenMind.Order.Domain.ValueObjects;
using OpenMind.Shared.Domain;

namespace OpenMind.Order.Domain.Entities;

/// <summary>
/// Entity representing an item in an order.
/// </summary>
public class OrderItem : Entity<Guid>
{
    public Guid ProductId { get; private set; }
    public string ProductName { get; private set; }
    public int Quantity { get; private set; }
    public Money UnitPrice { get; private set; }
    public Money TotalPrice => UnitPrice.Multiply(Quantity);

    private OrderItem() : base()
    {
        ProductName = string.Empty;
        UnitPrice = Money.Zero();
    }

    private OrderItem(Guid id, Guid productId, string productName, int quantity, Money unitPrice)
        : base(id)
    {
        ProductId = productId;
        ProductName = productName;
        Quantity = quantity;
        UnitPrice = unitPrice;
    }

    public static OrderItem Create(Guid productId, string productName, int quantity, Money unitPrice)
    {
        if (quantity <= 0)
            throw new ArgumentException("Quantity must be positive", nameof(quantity));

        return new OrderItem(Guid.NewGuid(), productId, productName, quantity, unitPrice);
    }

    public void UpdateQuantity(int newQuantity)
    {
        if (newQuantity <= 0)
            throw new ArgumentException("Quantity must be positive", nameof(newQuantity));

        Quantity = newQuantity;
        SetUpdatedAt();
    }
}
