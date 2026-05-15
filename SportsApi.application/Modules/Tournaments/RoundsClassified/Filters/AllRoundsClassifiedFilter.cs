using Ardalis.Specification;
using SportsApi.domain.Abstractions.Specifications;
using SportsApi.domain.Enums;
using SportsApi.domain.Modules.Tournaments;

namespace SportsApi.application.Modules.Tournaments.RoundsClassified.Filters;

public sealed class AllRoundsClassifiedFilter : PaginationSpecification<domain.Modules.Tournaments.RoundsClassified>
{
    public AllRoundsClassifiedFilter(
        int page,
        int perPage,
        Guid? teamParticipationId,
        Guid? tournamentId,
        MatchRound? round) : base(page, perPage)
    {
        if (teamParticipationId.HasValue)
            Query.Where(r => r.TeamParticipationId == teamParticipationId.Value);

        if (tournamentId.HasValue)
            Query.Where(r => r.TeamParticipation!.TournamentId == tournamentId.Value);

        if (round.HasValue)
            Query.Where(r => r.Round == round.Value);

        Query.OrderBy(r => r.RoundKey).ThenBy(r => r.GroupPosition);
        Query.Include(r => r.TeamParticipation!);
    }
}

