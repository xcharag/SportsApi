using Ardalis.Specification;
using SportsApi.domain.Enums;
using SportsApi.domain.Modules.Tournaments;

namespace SportsApi.application.Modules.Tournaments.RoundsClassified.Filters;

/// <summary>Gets all Final-round RoundsClassified for a team (to determine championships).</summary>
public sealed class FinalRoundsForTeamFilter : Specification<domain.Modules.Tournaments.RoundsClassified>
{
    public FinalRoundsForTeamFilter(Guid teamId)
    {
        Query.Where(r => r.Round == MatchRound.Final
                         && r.TeamParticipation!.TeamId == teamId);
    }
}
