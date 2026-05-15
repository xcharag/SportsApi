using SportsApi.domain.Abstractions.Messaging.Commands;

namespace SportsApi.application.Modules.Teams.Teams.Commands.DeleteTeam;

public class DeleteTeamCommandResult : ICommandResult
{
    public Guid Id { get; set; }
}

