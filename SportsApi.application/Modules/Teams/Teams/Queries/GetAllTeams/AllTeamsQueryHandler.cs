using SportsApi.application.Modules.Teams.Teams.Filters;
using SportsApi.domain.Abstractions.Dtos;
using SportsApi.domain.Abstractions.Messaging.Queries;
using SportsApi.domain.Abstractions.Persistence;
using SportsApi.domain.Modules.Teams;

namespace SportsApi.application.Modules.Teams.Teams.Queries.GetAllTeams;

public class AllTeamsQueryHandler(IRepository<Team> repository)
    : IQueryHandler<AllTeamsQuery, AllTeamsQueryResult>
{
    public async Task<Result<AllTeamsQueryResult>> HandleAsync(
        AllTeamsQuery query,
        CancellationToken cancellationToken)
    {
        var filter = new AllTeamsFilter(query.Page, query.PerPage, query.Name);

        var paginatedResult = await repository.GetPaginatedAsync(filter, cancellationToken);
        var totalCount      = await repository.CountBySpecificationAsync(filter, includeInactive: true,  cancellationToken);
        var activeCount     = await repository.CountBySpecificationAsync(filter, includeInactive: false, cancellationToken);

        return Result.Success(new AllTeamsQueryResult
        {
            Data          = paginatedResult,
            TotalCount    = totalCount,
            ActiveCount   = activeCount,
            InactiveCount = totalCount - activeCount
        });
    }
}

