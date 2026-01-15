using MediatR;

namespace OpenMind.Shared.MongoDb;

/// <summary>
/// MediatR pipeline behavior that dispatches domain events after command execution.
/// This ensures events are dispatched once at the end of the unit of work.
/// </summary>
public class DomainEventDispatchBehavior<TRequest, TResponse>(MongoDbContext dbContext)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        var response = await next();

        // Dispatch domain events after the handler completes
        await dbContext.SaveChangesAsync(cancellationToken);

        return response;
    }
}
