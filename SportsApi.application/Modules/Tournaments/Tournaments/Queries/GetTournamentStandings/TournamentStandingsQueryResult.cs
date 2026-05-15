using SportsApi.domain.Abstractions.Messaging.Queries;

namespace SportsApi.application.Modules.Tournaments.Tournaments.Queries.GetTournamentStandings;

public class TournamentStandingsQueryResult : IQueryResult
{
    /// <summary>One entry per group. Each group contains its standings rows sorted by Points desc.</summary>
    public List<GroupStandings> Groups { get; set; } = [];
}

public class GroupStandings
{
    public string                GroupKey  { get; set; } = string.Empty;
    public List<StandingRow>     Standings { get; set; } = [];
}

public class StandingRow
{
    public Guid    TeamParticipationId { get; set; }
    public string  DisplayName         { get; set; } = string.Empty;
    public string? LogoUrl             { get; set; }
    public int     Played              { get; set; }
    public int     Won                 { get; set; }
    public int     Drawn               { get; set; }
    public int     Lost                { get; set; }
    public int     GoalsFor            { get; set; }
    public int     GoalsAgainst        { get; set; }
    public int     GoalDifference      => GoalsFor - GoalsAgainst;
    public int     Points              => (Won * 3) + Drawn;
}
