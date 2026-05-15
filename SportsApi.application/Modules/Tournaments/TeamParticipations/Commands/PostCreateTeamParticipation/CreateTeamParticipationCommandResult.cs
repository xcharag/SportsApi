using SportsApi.domain.Abstractions.Messaging.Commands;

namespace SportsApi.application.Modules.Tournaments.TeamParticipations.Commands.PostCreateTeamParticipation;

public class CreateTeamParticipationCommandResult : ICommandResult
{
    public Guid   Id   { get; set; }
    public string Name { get; set; } = string.Empty;
}

