using SportsApi.domain.Abstractions.Messaging.Commands;

namespace SportsApi.application.Modules.Teams.Rosters.Commands.PostCreateRoster;

public class CreateRosterCommandResult : ICommandResult
{
    public Guid Id { get; set; }
}

