using SportsApi.domain.Abstractions.Messaging.Queries;
using SportsApi.domain.Enums;
using SportsApi.domain.Enums.Status;

namespace SportsApi.application.Modules.Tournaments.Tournaments.Queries.GetTournamentBracket;

public class TournamentBracketQueryResult : IQueryResult
{
    public List<BracketRound> Rounds { get; set; } = [];
}

public class BracketRound
{
    public MatchRound         Round     { get; set; }
    public string             RoundName { get; set; } = string.Empty;
    public List<BracketSlot>  Slots     { get; set; } = [];
}

public class BracketSlot
{
    /// <summary>The RoundKey that the winner of this slot advances to (= the two teams' NextRoundKey).</summary>
    public string?          WinnerAdvancesTo { get; set; }

    public BracketTeamEntry? HomeEntry { get; set; }
    public BracketTeamEntry? AwayEntry { get; set; }

    public BracketMatchSummary? Match  { get; set; }
    public BracketTeamEntry?    Winner { get; set; }
}

public class BracketTeamEntry
{
    public Guid    TeamParticipationId { get; set; }
    public string  DisplayName         { get; set; } = string.Empty;
    public string? LogoUrl             { get; set; }
    /// <summary>The RoundKey this team occupies in this round's slot.</summary>
    public string  RoundKey            { get; set; } = string.Empty;
    public bool    IsActive            { get; set; }
}

public class BracketMatchSummary
{
    public Guid        Id        { get; set; }
    public int         HomeScore { get; set; }
    public int         AwayScore { get; set; }
    public MatchStatus Status    { get; set; }
}
