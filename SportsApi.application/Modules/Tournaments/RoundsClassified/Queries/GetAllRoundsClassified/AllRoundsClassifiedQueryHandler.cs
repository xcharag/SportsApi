using SportsApi.application.Modules.Tournaments.RoundsClassified.Filters;
using SportsApi.domain.Abstractions.Dtos;
using SportsApi.domain.Abstractions.Messaging.Queries;
using SportsApi.domain.Abstractions.Persistence;

namespace SportsApi.application.Modules.Tournaments.RoundsClassified.Queries.GetAllRoundsClassified;

public class AllRoundsClassifiedQueryHandler(
    IRepository<domain.Modules.Tournaments.RoundsClassified> repository)
    : IQueryHandler<AllRoundsClassifiedQuery, AllRoundsClassifiedQueryResult>
{
    public async Task<Result<AllRoundsClassifiedQueryResult>> HandleAsync(
        AllRoundsClassifiedQuery query,
        CancellationToken cancellationToken)
    {
        var filter = new AllRoundsClassifiedFilter(
            query.Page, query.PerPage,
            query.TeamParticipationId,
            query.TournamentId,
            query.Round);

        var paginated    = await repository.GetPaginatedAsync(filter, cancellationToken);
        var totalCount   = await repository.CountBySpecificationAsync(filter, includeInactive: true,  cancellationToken);
        var activeCount  = await repository.CountBySpecificationAsync(filter, includeInactive: false, cancellationToken);

        return Result.Success(new AllRoundsClassifiedQueryResult
        {
            Data         = paginated,
            TotalCount   = totalCount,
            ActiveCount  = activeCount,
            InactiveCount = totalCount - activeCount
        });
    }
}

