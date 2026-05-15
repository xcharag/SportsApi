using SportsApi.domain.Abstractions.Messaging.Queries;

namespace SportsApi.application.Modules.Teams.Players.Queries.GetPlayerStats;

public class PlayerStatsQueryResult : IQueryResult
{
    public Guid    PlayerId  { get; set; }
    public string  FullName  { get; set; } = string.Empty;

    /// <summary>Career totals across all tournaments.</summary>
    public PlayerStatTotals Career { get; set; } = new();

    /// <summary>Per-tournament breakdown.</summary>
    public List<PlayerTournamentStats> Tournaments { get; set; } = [];
}

public class PlayerTournamentStats
{
    public Guid   TournamentId   { get; set; }
    public string TournamentName { get; set; } = string.Empty;
    public Guid   TeamParticipationId { get; set; }
    public string TeamName       { get; set; } = string.Empty;
    public string? ShirtName     { get; set; }
    public int?   ShirtNumber    { get; set; }
    public PlayerStatTotals Stats { get; set; } = new();
}

public class PlayerStatTotals
{
    public int Goals       { get; set; }
    public int YellowCards { get; set; }
    public int RedCards    { get; set; }
    public int Penalties   { get; set; }
    public int Offsides    { get; set; }
    public int Corners     { get; set; }
    public int FreeKicks   { get; set; }
}
