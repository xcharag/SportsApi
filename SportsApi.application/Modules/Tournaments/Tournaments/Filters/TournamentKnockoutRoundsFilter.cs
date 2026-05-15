using Ardalis.Specification;
using SportsApi.domain.Enums;
using SportsApi.domain.Modules.Tournaments;

namespace SportsApi.application.Modules.Tournaments.Tournaments.Filters;

/// <summary>Gets all knockout-round (R16+) RoundsClassified for a tournament, including inactive (eliminated) teams.</summary>
public sealed class TournamentKnockoutRoundsFilter : Specification<domain.Modules.Tournaments.RoundsClassified>
{
    public TournamentKnockoutRoundsFilter(Guid tournamentId)
    {
        Query.Where(r => r.TeamParticipation!.TournamentId == tournamentId
                         && r.Round >= MatchRound.R16);

        Query.Include(r => r.TeamParticipation!);
        Query.OrderBy(r => r.Round).ThenBy(r => r.RoundKey);
    }
}
