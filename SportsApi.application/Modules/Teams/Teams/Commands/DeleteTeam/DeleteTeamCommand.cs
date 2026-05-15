using SportsApi.domain.Abstractions.Messaging.Commands;

namespace SportsApi.application.Modules.Teams.Teams.Commands.DeleteTeam;

public class DeleteTeamCommand : ICommand<DeleteTeamCommandResult>
{
    public Guid Id { get; set; }
    public bool HardDelete { get; set; } = false;
}

