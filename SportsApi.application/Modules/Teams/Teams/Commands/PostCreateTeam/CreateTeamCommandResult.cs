using SportsApi.domain.Abstractions.Messaging.Commands;

namespace SportsApi.application.Modules.Teams.Teams.Commands.PostCreateTeam;

public class CreateTeamCommandResult : ICommandResult
{
    public Guid Id { get; set; }
    public string DefaultName { get; set; } = string.Empty;
}

