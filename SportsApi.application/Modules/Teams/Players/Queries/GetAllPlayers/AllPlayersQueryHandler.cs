using SportsApi.application.Modules.Teams.Players.Filters;
using SportsApi.domain.Abstractions.Dtos;
using SportsApi.domain.Abstractions.Messaging.Queries;
using SportsApi.domain.Abstractions.Persistence;
using SportsApi.domain.Modules.Teams;

namespace SportsApi.application.Modules.Teams.Players.Queries.GetAllPlayers;

public class AllPlayersQueryHandler(IRepository<Player> repository)
    : IQueryHandler<AllPlayersQuery, AllPlayersQueryResult>
{
    public async Task<Result<AllPlayersQueryResult>> HandleAsync(
        AllPlayersQuery query,
        CancellationToken cancellationToken)
    {
        var filter = new AllPlayersFilter(query.Page, query.PerPage, query.Name, query.IsForeigner);

        var paginatedResult = await repository.GetPaginatedAsync(filter, cancellationToken);
        var totalCount      = await repository.CountBySpecificationAsync(filter, includeInactive: true,  cancellationToken);
        var activeCount     = await repository.CountBySpecificationAsync(filter, includeInactive: false, cancellationToken);

        return Result.Success(new AllPlayersQueryResult
        {
            Data          = paginatedResult,
            TotalCount    = totalCount,
            ActiveCount   = activeCount,
            InactiveCount = totalCount - activeCount
        });
    }
}

