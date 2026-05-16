using SportsApi.application.Modules.Matches.Matches.Filters;
using SportsApi.domain.Abstractions.Dtos;
using SportsApi.domain.Abstractions.Messaging.Queries;
using SportsApi.domain.Abstractions.Persistence;
using SportsApi.domain.Modules.Matches;

namespace SportsApi.application.Modules.Matches.Matches.Queries.GetAllMatches;

public class AllMatchesQueryHandler(IRepository<Match> repository)
    : IQueryHandler<AllMatchesQuery, AllMatchesQueryResult>
{
    public async Task<Result<AllMatchesQueryResult>> HandleAsync(
        AllMatchesQuery query,
        CancellationToken cancellationToken)
    {
        var filter = new AllMatchesFilter(
            query.Page, query.PerPage,
            query.HomeTeamId, query.AwayTeamId, query.TeamId,
            query.Status, query.FromDate, query.ToDate);

        var paginatedResult = await repository.GetPaginatedAsync(filter, cancellationToken);
        var totalCount      = await repository.CountBySpecificationAsync(filter, includeInactive: true,  cancellationToken);
        var activeCount     = await repository.CountBySpecificationAsync(filter, includeInactive: false, cancellationToken);

        return Result.Success(new AllMatchesQueryResult
        {
            Data          = paginatedResult,
            TotalCount    = totalCount,
            ActiveCount   = activeCount,
            InactiveCount = totalCount - activeCount
        });
    }
}

