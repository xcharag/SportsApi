using SportsApi.domain.Abstractions.Messaging.Commands;

namespace SportsApi.application.Modules.Teams.Teams.Commands.PutUpdateTeam;

public class UpdateTeamCommandResult : ICommandResult
{
    public Guid Id { get; set; }
}

