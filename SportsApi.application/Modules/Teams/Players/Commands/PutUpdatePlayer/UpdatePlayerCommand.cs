using SportsApi.domain.Abstractions.Messaging.Commands;

namespace SportsApi.application.Modules.Teams.Players.Commands.PutUpdatePlayer;

public class UpdatePlayerCommand : ICommand<UpdatePlayerCommandResult>
{
    public Guid    Id          { get; set; }
    public string? FullName    { get; set; }
    public string? Ci          { get; set; }
    public string? PhoneNumber { get; set; }
    public bool?   IsForeigner { get; set; }
}

