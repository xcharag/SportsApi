using SportsApi.domain.Abstractions.Messaging.Commands;

namespace SportsApi.application.Modules.Teams.Rosters.Commands.PutUpdateRoster;

public class UpdateRosterCommandResult : ICommandResult
{
    public Guid Id { get; set; }
}

