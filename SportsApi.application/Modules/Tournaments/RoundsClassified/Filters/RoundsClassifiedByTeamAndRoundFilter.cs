using Ardalis.Specification;
using SportsApi.domain.Enums;
using SportsApi.domain.Modules.Tournaments;

namespace SportsApi.application.Modules.Tournaments.RoundsClassified.Filters;

/// <summary>Gets the active RoundsClassified entry for a specific team participation at a specific round.</summary>
public sealed class RoundsClassifiedByTeamAndRoundFilter : Specification<domain.Modules.Tournaments.RoundsClassified>
{
    public RoundsClassifiedByTeamAndRoundFilter(Guid teamParticipationId, MatchRound round)
    {
        Query.Where(r => r.TeamParticipationId == teamParticipationId && r.Round == round);
    }
}
