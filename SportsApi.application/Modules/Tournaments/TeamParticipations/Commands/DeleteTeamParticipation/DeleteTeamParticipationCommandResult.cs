using SportsApi.domain.Abstractions.Messaging.Commands;

namespace SportsApi.application.Modules.Tournaments.TeamParticipations.Commands.DeleteTeamParticipation;

public class DeleteTeamParticipationCommandResult : ICommandResult
{
    public Guid Id { get; set; }
}

