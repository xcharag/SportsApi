using SportsApi.domain.Abstractions.Messaging.Commands;

namespace SportsApi.application.Modules.Teams.Rosters.Commands.EnrollPlayers;

public class EnrollPlayersCommandResult : ICommandResult
{
    public int EnrolledCount { get; set; }
    public List<EnrolledPlayerSummary> Players { get; set; } = [];
}

public class EnrolledPlayerSummary
{
    public Guid   RosterId   { get; set; }
    public Guid   PlayerId   { get; set; }
    public int?   ShirtNumber { get; set; }
    public string? ShirtName  { get; set; }
}

