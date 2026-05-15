using SportsApi.application.Modules.Teams.Rosters.Filters;
using SportsApi.domain.Abstractions.Dtos;
using SportsApi.domain.Abstractions.Messaging.Queries;
using SportsApi.domain.Abstractions.Persistence;
using SportsApi.domain.Modules.Teams;

namespace SportsApi.application.Modules.Teams.Rosters.Queries.GetAllRosters;

public class AllRostersQueryHandler(IRepository<Roster> repository)
    : IQueryHandler<AllRostersQuery, AllRostersQueryResult>
{
    public async Task<Result<AllRostersQueryResult>> HandleAsync(
        AllRostersQuery query,
        CancellationToken cancellationToken)
    {
        var filter = new AllRostersFilter(
            query.Page, query.PerPage, query.TeamParticipationId, query.PlayerId);

        var paginatedResult = await repository.GetPaginatedAsync(filter, cancellationToken);
        var totalCount      = await repository.CountBySpecificationAsync(filter, includeInactive: true,  cancellationToken);
        var activeCount     = await repository.CountBySpecificationAsync(filter, includeInactive: false, cancellationToken);

        return Result.Success(new AllRostersQueryResult
        {
            Data          = paginatedResult,
            TotalCount    = totalCount,
            ActiveCount   = activeCount,
            InactiveCount = totalCount - activeCount
        });
    }
}

