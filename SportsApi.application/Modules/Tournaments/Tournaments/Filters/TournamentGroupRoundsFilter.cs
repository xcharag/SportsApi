using Ardalis.Specification;
using SportsApi.domain.Enums;
using SportsApi.domain.Modules.Tournaments;

namespace SportsApi.application.Modules.Tournaments.Tournaments.Filters;

/// <summary>Gets all active Group-round RoundsClassified entries for a tournament, with TeamParticipation.</summary>
public sealed class TournamentGroupRoundsFilter : Specification<domain.Modules.Tournaments.RoundsClassified>
{
    public TournamentGroupRoundsFilter(Guid tournamentId, string? groupKey = null)
    {
        Query.Where(r => r.TeamParticipation!.TournamentId == tournamentId && r.Round == MatchRound.Group);

        if (!string.IsNullOrWhiteSpace(groupKey))
            Query.Where(r => r.RoundKey == groupKey);

        Query.Include(r => r.TeamParticipation!);
        Query.OrderBy(r => r.RoundKey).ThenBy(r => r.GroupPosition);
    }
}
