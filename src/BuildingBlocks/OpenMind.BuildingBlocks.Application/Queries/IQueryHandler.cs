using MediatR;

namespace OpenMind.BuildingBlocks.Application.Queries;

/// <summary>
/// Interface for query handlers.
/// </summary>
public interface IQueryHandler<TQuery, TResult> : IRequestHandler<TQuery, QueryResult<TResult>>
    where TQuery : IQuery<TResult>
{
}
