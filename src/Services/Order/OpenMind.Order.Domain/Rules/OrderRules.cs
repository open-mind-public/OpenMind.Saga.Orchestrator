using OpenMind.BuildingBlocks.Domain;
using OpenMind.Order.Domain.Enums;

namespace OpenMind.Order.Domain.Rules;

public class OrderMustBeInStatusRule(OrderStatus currentStatus, OrderStatus requiredStatus, string operation)
    : IBusinessRule
{
    public bool IsBroken() => currentStatus != requiredStatus;

    public string Message => $"Cannot {operation} order in {currentStatus} status. Required status: {requiredStatus}";
}

public class OrderMustBeInOneOfStatusesRule(OrderStatus currentStatus, OrderStatus[] allowedStatuses, string operation)
    : IBusinessRule
{
    public bool IsBroken() => !allowedStatuses.Contains(currentStatus);

    public string Message => $"Cannot {operation} order in {currentStatus} status. Allowed statuses: {string.Join(", ", allowedStatuses.Select(s => s.Name))}";
}

public class OrderMustHaveItemsRule(int itemCount) : IBusinessRule
{
    public bool IsBroken() => itemCount == 0;

    public string Message => "Order must have at least one item";
}

public class ShippingAddressMustBeProvidedRule(string? address) : IBusinessRule
{
    public bool IsBroken() => string.IsNullOrWhiteSpace(address);

    public string Message => "Shipping address must be provided";
}
