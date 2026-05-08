namespace SportsApi.domain.Abstractions.Messaging.Commands;

public interface ICommand
{
}

public interface ICommand<TResult> where TResult : ICommandResult
{
}

public interface ICommandResult
{
}