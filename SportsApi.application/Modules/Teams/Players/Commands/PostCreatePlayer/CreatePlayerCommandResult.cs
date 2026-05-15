using SportsApi.domain.Abstractions.Messaging.Commands;

namespace SportsApi.application.Modules.Teams.Players.Commands.PostCreatePlayer;

public class CreatePlayerCommandResult : ICommandResult
{
    public Guid   Id       { get; set; }
    public string FullName { get; set; } = string.Empty;
}

