using SportsApi.domain.Abstractions.Messaging.Commands;

namespace SportsApi.application.Modules.Matches.Matches.Commands.PutUpdateMatch;

public class UpdateMatchCommandResult : ICommandResult
{
    public Guid Id { get; set; }
}

