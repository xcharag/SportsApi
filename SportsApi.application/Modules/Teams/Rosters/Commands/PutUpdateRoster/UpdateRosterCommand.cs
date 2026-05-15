using SportsApi.domain.Abstractions.Messaging.Commands;

namespace SportsApi.application.Modules.Teams.Rosters.Commands.PutUpdateRoster;

public class UpdateRosterCommand : ICommand<UpdateRosterCommandResult>
{
    public Guid    Id          { get; set; }
    public int?    ShirtNumber { get; set; }
    public string? ShirtName   { get; set; }
}

