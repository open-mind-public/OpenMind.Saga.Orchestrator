namespace OpenMind.Shared.Domain;

/// <summary>
/// Base class for domain exceptions.
/// </summary>
public abstract class DomainException : Exception
{
    public string Code { get; }

    protected DomainException(string code, string message) : base(message)
    {
        Code = code;
    }

    protected DomainException(string code, string message, Exception innerException)
        : base(message, innerException)
    {
        Code = code;
    }
}

/// <summary>
/// Exception thrown when business rules are violated.
/// </summary>
public class BusinessRuleValidationException : DomainException
{
    public IBusinessRule BrokenRule { get; }

    public BusinessRuleValidationException(IBusinessRule brokenRule)
        : base(brokenRule.GetType().Name, brokenRule.Message)
    {
        BrokenRule = brokenRule;
    }
}
