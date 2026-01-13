using OpenMind.BuildingBlocks.Domain;
using OpenMind.Fulfillment.Domain.Enums;

namespace OpenMind.Fulfillment.Domain.Rules;

public class FulfillmentMustBeInStatusRule(FulfillmentStatus currentStatus, FulfillmentStatus requiredStatus, string operation)
    : IBusinessRule
{
    public bool IsBroken() => currentStatus != requiredStatus;

    public string Message => $"Cannot {operation} fulfillment in {currentStatus} status. Required status: {requiredStatus}";
}

public class FulfillmentMustBeInOneOfStatusesRule(FulfillmentStatus currentStatus, FulfillmentStatus[] allowedStatuses, string operation)
    : IBusinessRule
{
    public bool IsBroken() => !allowedStatuses.Contains(currentStatus);

    public string Message => $"Cannot {operation} fulfillment in {currentStatus} status. Allowed statuses: {string.Join(", ", allowedStatuses.Select(s => s.Name))}";
}

public class FulfillmentShippingAddressMustBeProvidedRule(string? address) : IBusinessRule
{
    public bool IsBroken() => string.IsNullOrWhiteSpace(address);

    public string Message => "Shipping address must be provided for fulfillment";
}

public class TrackingNumberMustBeProvidedRule(string? trackingNumber) : IBusinessRule
{
    public bool IsBroken() => string.IsNullOrWhiteSpace(trackingNumber);

    public string Message => "Tracking number must be provided";
}
