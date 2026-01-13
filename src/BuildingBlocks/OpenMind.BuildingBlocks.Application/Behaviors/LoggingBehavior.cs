using MediatR;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace OpenMind.BuildingBlocks.Application.Behaviors;

/// <summary>
/// Pipeline behavior for logging request handling.
/// </summary>
public class LoggingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly ILogger<LoggingBehavior<TRequest, TResponse>> _logger;

    public LoggingBehavior(ILogger<LoggingBehavior<TRequest, TResponse>> logger)
    {
        _logger = logger;
    }

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        var requestName = typeof(TRequest).Name;
        var requestGuid = Guid.NewGuid().ToString();

        _logger.LogInformation(
            "[START] {RequestName} [{RequestGuid}] - {@Request}",
            requestName, requestGuid, request);

        var stopwatch = Stopwatch.StartNew();

        try
        {
            var response = await next();

            stopwatch.Stop();

            _logger.LogInformation(
                "[END] {RequestName} [{RequestGuid}] - Completed in {ElapsedMilliseconds}ms",
                requestName, requestGuid, stopwatch.ElapsedMilliseconds);

            return response;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();

            _logger.LogError(ex,
                "[ERROR] {RequestName} [{RequestGuid}] - Failed in {ElapsedMilliseconds}ms",
                requestName, requestGuid, stopwatch.ElapsedMilliseconds);

            throw;
        }
    }
}
