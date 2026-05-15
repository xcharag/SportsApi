using Ardalis.Specification;
using SportsApi.domain.Enums;
using SportsApi.domain.Enums.Status;
using SportsApi.domain.Modules.Matches;

namespace SportsApi.application.Modules.Tournaments.Tournaments.Filters;

/// <summary>Gets all group-stage matches for a tournament, including team navigations.</summary>
public sealed class TournamentGroupMatchesFilter : Specification<Match>
{
    public TournamentGroupMatchesFilter(Guid tournamentId, bool finishedOnly = false)
    {
        Query.Where(m => m.HomeTeam.TournamentId == tournamentId && m.Round == MatchRound.Group);

        if (finishedOnly)
            Query.Where(m => m.Status == MatchStatus.Finished);

        Query.Include(m => m.HomeTeam);
        Query.Include(m => m.AwayTeam);
    }
}
