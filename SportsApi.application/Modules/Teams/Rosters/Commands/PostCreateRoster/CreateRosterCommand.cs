using SportsApi.domain.Abstractions.Messaging.Commands;

namespace SportsApi.application.Modules.Teams.Rosters.Commands.PostCreateRoster;

public sealed record CreateRosterCommand : ICommand<CreateRosterCommandResult>
{
    public int?   ShirtNumber        { get; set; }
    public string? ShirtName         { get; set; }
    public Guid   PlayerId           { get; set; }
    public Guid   TeamParticipationId { get; set; }
}

