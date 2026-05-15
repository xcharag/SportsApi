using Ardalis.Specification;
using SportsApi.domain.Enums;
using SportsApi.domain.Modules.Matches;

namespace SportsApi.application.Modules.Tournaments.Tournaments.Filters;

/// <summary>Gets all knockout-round (R16+) matches for a tournament, including team navigations.</summary>
public sealed class TournamentKnockoutMatchesFilter : Specification<Match>
{
    public TournamentKnockoutMatchesFilter(Guid tournamentId)
    {
        Query.Where(m => m.HomeTeam.TournamentId == tournamentId && m.Round >= MatchRound.R16);

        Query.Include(m => m.HomeTeam);
        Query.Include(m => m.AwayTeam);
        Query.OrderBy(m => m.Round).ThenBy(m => m.MatchDate);
    }
}
