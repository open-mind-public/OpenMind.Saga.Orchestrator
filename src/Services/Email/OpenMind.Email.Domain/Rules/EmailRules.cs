using OpenMind.BuildingBlocks.Domain;

namespace OpenMind.Email.Domain.Rules;

public class EmailRecipientMustBeValidRule(string? email) : IBusinessRule
{
    public bool IsBroken() => string.IsNullOrWhiteSpace(email) || !email.Contains('@');

    public string Message => "Email recipient must be a valid email address";
}

public class EmailSubjectMustBeProvidedRule(string? subject) : IBusinessRule
{
    public bool IsBroken() => string.IsNullOrWhiteSpace(subject);

    public string Message => "Email subject must be provided";
}

public class EmailBodyMustBeProvidedRule(string? body) : IBusinessRule
{
    public bool IsBroken() => string.IsNullOrWhiteSpace(body);

    public string Message => "Email body must be provided";
}
