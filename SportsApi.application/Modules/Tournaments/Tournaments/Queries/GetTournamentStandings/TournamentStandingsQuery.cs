using SportsApi.domain.Abstractions.Messaging.Queries;

namespace SportsApi.application.Modules.Tournaments.Tournaments.Queries.GetTournamentStandings;

public class TournamentStandingsQuery : IQuery<TournamentStandingsQueryResult>
{
    public Guid    TournamentId { get; set; }
    /// <summary>Filter to a specific group key (e.g. "A", "B"). Omit to return all groups.</summary>
    public string? GroupKey     { get; set; }
}
