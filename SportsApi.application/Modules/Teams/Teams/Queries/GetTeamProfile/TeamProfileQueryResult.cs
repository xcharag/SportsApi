using SportsApi.domain.Abstractions.Messaging.Queries;

namespace SportsApi.application.Modules.Teams.Teams.Queries.GetTeamProfile;

public class TeamProfileQueryResult : IQueryResult
{
    public Guid    TeamId         { get; set; }
    public string  DefaultName    { get; set; } = string.Empty;
    public string? DefaultLogoUrl { get; set; }

    /// <summary>Per-tournament history, newest first.</summary>
    public List<TeamTournamentHistory> TournamentHistory { get; set; } = [];

    /// <summary>Historic top scorers across all tournaments for this team.</summary>
    public List<TeamTopScorerRow> TopScorers { get; set; } = [];

    /// <summary>Career aggregated stats (goals, cards, etc.) for this team across all tournaments.</summary>
    public TeamCareerStats CareerStats { get; set; } = new();

    /// <summary>Aggregated win/draw/loss record and goals across all finished matches.</summary>
    public TeamRecord Record { get; set; } = new();
}

public class TeamTournamentHistory
{
    public Guid    TournamentId        { get; set; }
    public string  TournamentName      { get; set; } = string.Empty;
    public Guid    TeamParticipationId { get; set; }
    public string  ParticipationName   { get; set; } = string.Empty;
    public string? LogoUrl             { get; set; }
    /// <summary>True if the team won the tournament (Final round RC still active).</summary>
    public bool   IsChampion          { get; set; }
}

public class TeamTopScorerRow
{
    public Guid   PlayerId    { get; set; }
    public string PlayerName  { get; set; } = string.Empty;
    public int    TotalGoals  { get; set; }
}

public class TeamCareerStats
{
    public int Goals       { get; set; }
    public int YellowCards { get; set; }
    public int RedCards    { get; set; }
    public int Penalties   { get; set; }
}

public class TeamRecord
{
    public int Played       { get; set; }
    public int Won          { get; set; }
    public int Drawn        { get; set; }
    public int Lost         { get; set; }
    public int GoalsFor     { get; set; }
    public int GoalsAgainst { get; set; }
}

