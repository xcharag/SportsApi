using Ardalis.Specification;
using SportsApi.domain.Abstractions.Specifications;
using SportsApi.domain.Modules.Teams;

namespace SportsApi.application.Modules.Teams.Rosters.Filters;

public sealed class AllRostersFilter : PaginationSpecification<Roster>
{
    public AllRostersFilter(
        int page,
        int perPage,
        Guid? teamParticipationId,
        Guid? playerId) : base(page, perPage)
    {
        if (teamParticipationId.HasValue)
            Query.Where(r => r.TeamParticipationId == teamParticipationId.Value);

        if (playerId.HasValue)
            Query.Where(r => r.PlayerId == playerId.Value);

        Query.Include(r => r.Player);
        Query.Include(r => r.Team);
    }
}


