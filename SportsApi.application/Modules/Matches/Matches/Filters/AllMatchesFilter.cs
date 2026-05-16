using Ardalis.Specification;
using SportsApi.domain.Abstractions.Specifications;
using SportsApi.domain.Enums.Status;
using SportsApi.domain.Modules.Matches;

namespace SportsApi.application.Modules.Matches.Matches.Filters;

public sealed class AllMatchesFilter : PaginationSpecification<Match>
{
    public AllMatchesFilter(
        int page,
        int perPage,
        Guid? homeTeamId,
        Guid? awayTeamId,
        Guid? teamId,
        MatchStatus? status,
        DateTime? fromDate,
        DateTime? toDate) : base(page, perPage)
    {
        if (homeTeamId.HasValue)
            Query.Where(m => m.HomeTeamId == homeTeamId.Value);

        if (awayTeamId.HasValue)
            Query.Where(m => m.AwayTeamId == awayTeamId.Value);

        // Cross-tournament filter: any match where the team (by global Team.Id) participated
        if (teamId.HasValue)
            Query.Where(m => m.HomeTeam.TeamId == teamId.Value || m.AwayTeam.TeamId == teamId.Value);

        if (status.HasValue)
            Query.Where(m => m.Status == status.Value);

        if (fromDate.HasValue)
            Query.Where(m => m.MatchDate >= fromDate.Value);

        if (toDate.HasValue)
            Query.Where(m => m.MatchDate <= toDate.Value);

        Query.OrderBy(m => m.MatchDate);
        Query.Include(m => m.HomeTeam);
        Query.Include(m => m.AwayTeam);
    }
}


