using SportsApi.domain.Abstractions.Dtos;

namespace SportsApi.domain.Abstractions.Messaging.Commands;

public interface ICommandHandler<TCommand> where TCommand : ICommand
{
    Task<Result> HandleAsync(TCommand command, CancellationToken cancellationToken);
}

public interface ICommandHandler<TCommand, TResult>
    where TCommand : ICommand<TResult>
    where TResult : ICommandResult
{
    Task<Result<TResult>> HandleAsync(TCommand command, CancellationToken cancellationToken);
}