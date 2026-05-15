using System.Text.Json.Serialization;
using SportsApi.domain.Abstractions.Messaging.Commands;

namespace SportsApi.application.Modules.Tournaments.TeamParticipations.Commands.RegisterTeams;

/// <summary>
/// Registers one or more teams into a tournament.
/// Creates one TeamParticipation + one RoundsClassified (Group round) per entry.
/// TournamentId is supplied via the route — it is not part of the request body.
/// </summary>
public sealed record RegisterTeamsCommand : ICommand<RegisterTeamsCommandResult>
{
    /// <summary>Set by the controller from the {tournamentId} route segment — not required in the body.</summary>
    [JsonIgnore]
    public Guid TournamentId { get; set; }

    public List<RegisterTeamItem> Teams { get; set; } = [];
}

public class RegisterTeamItem
{
    public Guid    TeamId        { get; set; }
    /// <summary>Display name for this team in this tournament (may differ from the global team name).</summary>
    public string  Name          { get; set; } = string.Empty;
    public string? LogoUrl       { get; set; }
    /// <summary>Group / bracket key: "A", "B", "C" for group stage; "AA", "AB" etc. for knockouts.</summary>
    public string  RoundKey      { get; set; } = string.Empty;
    /// <summary>Seed / position within the group (optional).</summary>
    public int?    GroupPosition { get; set; }
    /// <summary>The RoundKey this team advances to if they qualify (e.g. "AA" for the first R16 slot).</summary>
    public string? NextRoundKey  { get; set; }
}

