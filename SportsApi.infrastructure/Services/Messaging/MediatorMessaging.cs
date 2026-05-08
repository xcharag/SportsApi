using Microsoft.Extensions.DependencyInjection;
using SportsApi.domain.Abstractions.Dtos;
using SportsApi.domain.Abstractions.Messaging;
using SportsApi.domain.Abstractions.Messaging.Commands;
using SportsApi.domain.Abstractions.Messaging.Queries;

namespace SportsApi.infrastructure.Services.Messaging;

public sealed class Mediator(IServiceProvider serviceProvider) : IMediator
{
    public Task<Result> SendCommandAsync<TCommand>(TCommand command, CancellationToken cancellationToken)
        where TCommand : ICommand
    {
        var handler = serviceProvider.GetRequiredService<ICommandHandler<TCommand>>();
        return handler.HandleAsync(command, cancellationToken);
    }

    public Task<Result<TResult>> SendCommandAsync<TCommand, TResult>(
        TCommand command,
        CancellationToken cancellationToken)
        where TCommand : ICommand<TResult>
        where TResult : ICommandResult
    {
        var handler = serviceProvider.GetRequiredService<ICommandHandler<TCommand, TResult>>();
        return handler.HandleAsync(command, cancellationToken);
    }

    public Task<Result<TResult>> SendQueryAsync<TQuery, TResult>(
        TQuery query,
        CancellationToken cancellationToken)
        where TQuery : IQuery<TResult>
    {
        var handler = serviceProvider.GetRequiredService<IQueryHandler<TQuery, TResult>>();
        return handler.HandleAsync(query, cancellationToken);
    }
}