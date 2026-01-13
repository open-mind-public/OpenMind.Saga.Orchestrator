using MediatR;

namespace OpenMind.Shared.Application.Queries;

/// <summary>
/// Marker interface for queries in the CQRS pattern.
/// Queries represent requests for data without side effects.
/// </summary>
public interface IQuery<TResult> : IRequest<QueryResult<TResult>>
{
}
