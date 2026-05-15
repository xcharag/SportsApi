using SportsApi.domain.Abstractions.Messaging.Commands;

namespace SportsApi.application.Modules.Tournaments.TeamParticipations.Commands.PutUpdateTeamParticipation;

public class UpdateTeamParticipationCommandResult : ICommandResult
{
    public Guid Id { get; set; }
}

