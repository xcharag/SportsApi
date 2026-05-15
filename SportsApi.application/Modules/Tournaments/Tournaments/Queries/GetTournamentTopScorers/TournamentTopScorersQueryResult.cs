using SportsApi.domain.Abstractions.Messaging.Queries;

namespace SportsApi.application.Modules.Tournaments.Tournaments.Queries.GetTournamentTopScorers;

public class TournamentTopScorersQueryResult : IQueryResult
{
    public List<TopScorerRow> TopScorers { get; set; } = [];
}

public class TopScorerRow
{
    public int    Rank                { get; set; }
    public Guid   PlayerId            { get; set; }
    public string PlayerName          { get; set; } = string.Empty;
    public Guid   RosterId            { get; set; }
    public Guid   TeamParticipationId { get; set; }
    public string TeamName            { get; set; } = string.Empty;
    public string? ShirtName          { get; set; }
    public int?   ShirtNumber         { get; set; }
    public int    Goals               { get; set; }
}
