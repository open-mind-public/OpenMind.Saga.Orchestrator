namespace OpenMind.BuildingBlocks.Domain;

/// <summary>
/// Interface for business rules.
/// Business rules encapsulate domain invariants that must be satisfied.
/// </summary>
public interface IBusinessRule
{
    bool IsBroken();
    string Message { get; }
}

/// <summary>
/// Extension methods for checking business rules.
/// </summary>
public static class BusinessRuleExtensions
{
    public static void CheckRule(this IBusinessRule rule)
    {
        if (rule.IsBroken())
        {
            throw new BusinessRuleValidationException(rule);
        }
    }
}
