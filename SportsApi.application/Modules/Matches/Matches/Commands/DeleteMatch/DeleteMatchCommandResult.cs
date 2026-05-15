using SportsApi.domain.Abstractions.Messaging.Commands;

namespace SportsApi.application.Modules.Matches.Matches.Commands.DeleteMatch;

public class DeleteMatchCommandResult : ICommandResult
{
    public Guid Id { get; set; }
}

