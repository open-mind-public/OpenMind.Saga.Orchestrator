namespace OpenMind.Shared.Application.Queries;

/// <summary>
/// Represents the result of a query execution.
/// </summary>
public class QueryResult<TResult>
{
    public bool IsSuccess { get; }
    public TResult? Data { get; }
    public string? ErrorMessage { get; }
    public string? ErrorCode { get; }

    private QueryResult(bool isSuccess, TResult? data = default, string? errorMessage = null, string? errorCode = null)
    {
        IsSuccess = isSuccess;
        Data = data;
        ErrorMessage = errorMessage;
        ErrorCode = errorCode;
    }

    public static QueryResult<TResult> Success(TResult data) => new(true, data);
    public static QueryResult<TResult> Failure(string errorMessage, string? errorCode = null)
        => new(false, default, errorMessage, errorCode);
    public static QueryResult<TResult> NotFound(string? message = null)
        => new(false, default, message ?? "Resource not found", "NOT_FOUND");
}
