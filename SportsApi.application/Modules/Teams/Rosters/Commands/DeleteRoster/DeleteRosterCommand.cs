using SportsApi.domain.Abstractions.Messaging.Commands;

namespace SportsApi.application.Modules.Teams.Rosters.Commands.DeleteRoster;

public class DeleteRosterCommand : ICommand<DeleteRosterCommandResult>
{
    public Guid Id { get; set; }
    public bool HardDelete { get; set; } = false;
}

