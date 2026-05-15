using SportsApi.domain.Abstractions.Messaging.Queries;
using SportsApi.domain.Enums;
using SportsApi.domain.Enums.Types;

namespace SportsApi.application.Modules.Teams.Players.Queries.GetPlayerProfile;

public class PlayerProfileQueryResult : IQueryResult
{
    public Guid    PlayerId    { get; set; }
    public string  FullName    { get; set; } = string.Empty;
    public string? Ci          { get; set; }
    public string? PhoneNumber { get; set; }
    public bool    IsForeigner { get; set; }

    /// <summary>Teams the player has played for.</summary>
    public List<PlayerTeamEntry> Teams { get; set; } = [];

    /// <summary>Aggregated career stats.</summary>
    public PlayerCareerStats Career { get; set; } = new();
}

public class PlayerTeamEntry
{
    public Guid    TeamParticipationId { get; set; }
    public string  TeamName            { get; set; } = string.Empty;
    public string? LogoUrl             { get; set; }
    public Guid    TournamentId        { get; set; }
    public string  TournamentName      { get; set; } = string.Empty;
    public string? ShirtName           { get; set; }
    public int?    ShirtNumber         { get; set; }
    public List<PlayerEventRecord> Events { get; set; } = [];
}

public class PlayerEventRecord
{
    public Guid      EventId   { get; set; }
    public EventType EventType { get; set; }
    public int       Minute    { get; set; }
    public Guid      MatchId   { get; set; }
}

public class PlayerCareerStats
{
    public int Goals       { get; set; }
    public int YellowCards { get; set; }
    public int RedCards    { get; set; }
    public int Penalties   { get; set; }
    public int Offsides    { get; set; }
    public int Corners     { get; set; }
    public int FreeKicks   { get; set; }
}
