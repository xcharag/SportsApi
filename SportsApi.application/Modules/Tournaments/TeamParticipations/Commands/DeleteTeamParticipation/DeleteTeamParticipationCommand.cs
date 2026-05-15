using SportsApi.domain.Abstractions.Messaging.Commands;

namespace SportsApi.application.Modules.Tournaments.TeamParticipations.Commands.DeleteTeamParticipation;

public class DeleteTeamParticipationCommand : ICommand<DeleteTeamParticipationCommandResult>
{
    public Guid Id { get; set; }
    public bool HardDelete { get; set; } = false;
}

