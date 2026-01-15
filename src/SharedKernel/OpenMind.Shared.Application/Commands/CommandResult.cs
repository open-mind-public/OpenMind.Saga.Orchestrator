namespace OpenMind.Shared.Application.Commands;

/// <summary>
/// Represents the result of a command execution.
/// </summary>
public class CommandResult
{
    public bool IsSuccess { get; }
    public string? ErrorMessage { get; }
    public string? ErrorCode { get; }

    protected CommandResult(bool isSuccess, string? errorMessage = null, string? errorCode = null)
    {
        IsSuccess = isSuccess;
        ErrorMessage = errorMessage;
        ErrorCode = errorCode;
    }

    public static CommandResult Success() => new(true);
    public static CommandResult Failure(string errorMessage, string? errorCode = null)
        => new(false, errorMessage, errorCode);
}

/// <summary>
/// Represents the result of a command execution with a typed result.
/// </summary>
public class CommandResult<TResult> : CommandResult
{
    public TResult? Data { get; }

    private CommandResult(bool isSuccess, TResult? data = default, string? errorMessage = null, string? errorCode = null)
        : base(isSuccess, errorMessage, errorCode)
    {
        Data = data;
    }

    public static CommandResult<TResult> Success(TResult data) => new(true, data);
    public new static CommandResult<TResult> Failure(string errorMessage, string? errorCode = null)
        => new(false, default, errorMessage, errorCode);
}
