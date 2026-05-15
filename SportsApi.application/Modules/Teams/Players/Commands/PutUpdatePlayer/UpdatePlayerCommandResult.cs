using SportsApi.domain.Abstractions.Messaging.Commands;

namespace SportsApi.application.Modules.Teams.Players.Commands.PutUpdatePlayer;

public class UpdatePlayerCommandResult : ICommandResult
{
    public Guid Id { get; set; }
}

