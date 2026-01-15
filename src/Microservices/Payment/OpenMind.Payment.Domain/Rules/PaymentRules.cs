using OpenMind.Payment.Domain.Enums;
using OpenMind.Shared.Domain;

namespace OpenMind.Payment.Domain.Rules;

public class PaymentMustBeInStatusRule(PaymentStatus currentStatus, PaymentStatus requiredStatus, string operation)
    : IBusinessRule
{
    public bool IsBroken() => !Equals(currentStatus, requiredStatus);

    public string Message => $"Cannot {operation} payment in {currentStatus} status. Required status: {requiredStatus}";
}

public class PaymentAmountMustBePositiveRule(decimal amount) : IBusinessRule
{
    public bool IsBroken() => amount <= 0;

    public string Message => $"Payment amount must be positive. Provided: {amount}";
}

public class CardNumberMustBeValidRule(string cardNumber) : IBusinessRule
{
    public bool IsBroken() => string.IsNullOrWhiteSpace(cardNumber) || cardNumber.Length < 13;

    public string Message => "Card number must be at least 13 digits";
}

public class PaymentMethodMustBeProvidedRule(string paymentMethod) : IBusinessRule
{
    public bool IsBroken() => string.IsNullOrWhiteSpace(paymentMethod);

    public string Message => "Payment method must be provided";
}
