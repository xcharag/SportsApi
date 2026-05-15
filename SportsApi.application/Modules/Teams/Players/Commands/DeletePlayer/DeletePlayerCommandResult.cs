using SportsApi.domain.Abstractions.Messaging.Commands;

namespace SportsApi.application.Modules.Teams.Players.Commands.DeletePlayer;

public class DeletePlayerCommandResult : ICommandResult
{
    public Guid Id { get; set; }
}

