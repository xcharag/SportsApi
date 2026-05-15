using SportsApi.domain.Abstractions.Messaging.Commands;

namespace SportsApi.application.Modules.Teams.Rosters.Commands.DeleteRoster;

public class DeleteRosterCommandResult : ICommandResult
{
    public Guid Id { get; set; }
}

