using MediatR;

namespace OpenMind.Shared.Application.Commands;

/// <summary>
/// Marker interface for commands in the CQRS pattern.
/// Commands represent intentions to change state.
/// </summary>
public interface ICommand : IRequest<CommandResult>
{
}

/// <summary>
/// Interface for commands that return a typed result.
/// </summary>
public interface ICommand<TResult> : IRequest<CommandResult<TResult>>
{
}
