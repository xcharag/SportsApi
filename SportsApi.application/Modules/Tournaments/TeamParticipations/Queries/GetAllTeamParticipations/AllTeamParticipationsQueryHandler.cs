using SportsApi.application.Modules.Tournaments.TeamParticipations.Filters;
using SportsApi.domain.Abstractions.Dtos;
using SportsApi.domain.Abstractions.Messaging.Queries;
using SportsApi.domain.Abstractions.Persistence;
using SportsApi.domain.Modules.Tournaments;

namespace SportsApi.application.Modules.Tournaments.TeamParticipations.Queries.GetAllTeamParticipations;

public class AllTeamParticipationsQueryHandler(IRepository<TeamParticipation> repository)
    : IQueryHandler<AllTeamParticipationsQuery, AllTeamParticipationsQueryResult>
{
    public async Task<Result<AllTeamParticipationsQueryResult>> HandleAsync(
        AllTeamParticipationsQuery query,
        CancellationToken cancellationToken)
    {
        var filter = new AllTeamParticipationsFilter(
            query.Page, query.PerPage, query.TournamentId, query.TeamId, query.Name);

        var paginatedResult = await repository.GetPaginatedAsync(filter, cancellationToken);
        var totalCount      = await repository.CountBySpecificationAsync(filter, includeInactive: true,  cancellationToken);
        var activeCount     = await repository.CountBySpecificationAsync(filter, includeInactive: false, cancellationToken);

        return Result.Success(new AllTeamParticipationsQueryResult
        {
            Data          = paginatedResult,
            TotalCount    = totalCount,
            ActiveCount   = activeCount,
            InactiveCount = totalCount - activeCount
        });
    }
}

