using System.Diagnostics;
using MediatR;
using Microsoft.Extensions.Logging;

namespace OpenMind.Shared.Application.Behaviors;

/// <summary>
/// Pipeline behavior for logging request handling.
/// </summary>
public class LoggingBehavior<TRequest, TResponse>(ILogger<LoggingBehavior<TRequest, TResponse>> logger)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        var requestName = typeof(TRequest).Name;
        var requestGuid = Guid.NewGuid().ToString();

        logger.LogInformation(
            "[START] {RequestName} [{RequestGuid}] - {@Request}",
            requestName, requestGuid, request);

        var stopwatch = Stopwatch.StartNew();

        try
        {
            var response = await next();

            stopwatch.Stop();

            logger.LogInformation(
                "[END] {RequestName} [{RequestGuid}] - Completed in {ElapsedMilliseconds}ms",
                requestName, requestGuid, stopwatch.ElapsedMilliseconds);

            return response;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();

            logger.LogError(ex,
                "[ERROR] {RequestName} [{RequestGuid}] - Failed in {ElapsedMilliseconds}ms",
                requestName, requestGuid, stopwatch.ElapsedMilliseconds);

            throw;
        }
    }
}
