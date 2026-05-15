using SportsApi.domain.Abstractions.Messaging.Commands;

namespace SportsApi.application.Modules.Teams.Players.Commands.DeletePlayer;

public class DeletePlayerCommand : ICommand<DeletePlayerCommandResult>
{
    public Guid Id { get; set; }
    public bool HardDelete { get; set; } = false;
}

