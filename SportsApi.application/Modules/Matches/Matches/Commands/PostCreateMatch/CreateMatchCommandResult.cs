using SportsApi.domain.Abstractions.Messaging.Commands;

namespace SportsApi.application.Modules.Matches.Matches.Commands.PostCreateMatch;

public class CreateMatchCommandResult : ICommandResult
{
    public Guid Id { get; set; }
}

