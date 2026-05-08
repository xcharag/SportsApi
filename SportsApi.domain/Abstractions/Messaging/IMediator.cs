using SportsApi.domain.Abstractions.Dtos;
using SportsApi.domain.Abstractions.Messaging.Commands;
using SportsApi.domain.Abstractions.Messaging.Queries;

namespace SportsApi.domain.Abstractions.Messaging;

public interface IMediator
{
    Task<Result> SendCommandAsync<TCommand>(TCommand command, CancellationToken cancellationToken)
        where TCommand : ICommand;

    Task<Result<TResult>> SendCommandAsync<TCommand, TResult>(TCommand command, CancellationToken cancellationToken)
        where TCommand : ICommand<TResult>
        where TResult : ICommandResult;

    Task<Result<TResult>> SendQueryAsync<TQuery, TResult>(TQuery query, CancellationToken cancellationToken)
        where TQuery : IQuery<TResult>;
}