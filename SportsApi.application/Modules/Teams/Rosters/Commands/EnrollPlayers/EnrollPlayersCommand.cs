using System.Text.Json.Serialization;
using SportsApi.domain.Abstractions.Messaging.Commands;

namespace SportsApi.application.Modules.Teams.Rosters.Commands.EnrollPlayers;

/// <summary>
/// Enrolls one or more players into a TeamParticipation (team-in-tournament roster).
/// Creates one Roster record per player.
/// TeamParticipationId is supplied via the route — it is not part of the request body.
/// </summary>
public sealed record EnrollPlayersCommand : ICommand<EnrollPlayersCommandResult>
{
    /// <summary>Set by the controller from the {teamParticipationId} route segment — not required in the body.</summary>
    [JsonIgnore]
    public Guid TeamParticipationId { get; set; }

    public List<EnrollPlayerItem> Players { get; set; } = [];
}

public class EnrollPlayerItem
{
    public Guid    PlayerId    { get; set; }
    public int?    ShirtNumber { get; set; }
    public string? ShirtName   { get; set; }
}

