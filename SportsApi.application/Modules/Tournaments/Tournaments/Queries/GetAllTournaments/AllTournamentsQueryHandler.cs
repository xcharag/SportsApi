using SportsApi.application.Modules.Tournaments.Tournaments.Filters;
using SportsApi.domain.Abstractions.Dtos;
using SportsApi.domain.Abstractions.Messaging.Queries;
using SportsApi.domain.Abstractions.Persistence;
using SportsApi.domain.Modules.Tournaments;

namespace SportsApi.application.Modules.Tournaments.Tournaments.Queries.GetAllTournaments;

public class AllTournamentsQueryHandler(IRepository<Tournament> repository) 
    : IQueryHandler<AllTournamentsQuery, AllTournamentsQueryResult>
{
    public async Task<Result<AllTournamentsQueryResult>> HandleAsync(
        AllTournamentsQuery query, 
        CancellationToken cancellationToken)
    {
        var filter = new AllTournamentsFilter(query.Page, query.PerPage, query.Name, query.InitDate, query.EndDate);
        
        var paginatedResult = await repository.GetPaginatedAsync(filter, cancellationToken);
        
        var totalCount = await repository.CountBySpecificationAsync(filter, includeInactive: true, cancellationToken);
        var activeCount = await repository.CountBySpecificationAsync(filter, includeInactive: false, cancellationToken);
        var inactiveCount = totalCount - activeCount;
        
        return Result.Success(new AllTournamentsQueryResult
        {
            Data = paginatedResult,
            TotalCount = totalCount,
            ActiveCount = activeCount,
            InactiveCount = inactiveCount
        });
    }
}