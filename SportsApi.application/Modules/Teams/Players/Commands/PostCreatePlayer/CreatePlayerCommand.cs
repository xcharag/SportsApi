using SportsApi.domain.Abstractions.Messaging.Commands;

namespace SportsApi.application.Modules.Teams.Players.Commands.PostCreatePlayer;

public sealed record CreatePlayerCommand : ICommand<CreatePlayerCommandResult>
{
    public string  FullName     { get; set; } = string.Empty;
    public string? Ci           { get; set; }
    public string? PhoneNumber  { get; set; }
    public bool    IsForeigner  { get; set; }
}

