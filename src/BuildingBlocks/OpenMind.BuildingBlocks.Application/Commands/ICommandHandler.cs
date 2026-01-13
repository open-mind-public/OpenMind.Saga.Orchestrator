using MediatR;

namespace OpenMind.BuildingBlocks.Application.Commands;

/// <summary>
/// Interface for command handlers.
/// </summary>
public interface ICommandHandler<TCommand> : IRequestHandler<TCommand, CommandResult>
    where TCommand : ICommand
{
}

/// <summary>
/// Interface for command handlers with typed results.
/// </summary>
public interface ICommandHandler<TCommand, TResult> : IRequestHandler<TCommand, CommandResult<TResult>>
    where TCommand : ICommand<TResult>
{
}
