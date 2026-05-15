using SportsApi.application.Modules.Matches.Events.Filters;
using SportsApi.domain.Abstractions.Dtos;
using SportsApi.domain.Abstractions.Messaging.Queries;
using SportsApi.domain.Abstractions.Persistence;
using SportsApi.domain.Modules.Matches;

namespace SportsApi.application.Modules.Matches.Events.Queries.GetAllEvents;

public class AllEventsQueryHandler(IRepository<Event> repository)
    : IQueryHandler<AllEventsQuery, AllEventsQueryResult>
{
    public async Task<Result<AllEventsQueryResult>> HandleAsync(
        AllEventsQuery query,
        CancellationToken cancellationToken)
    {
        var filter = new AllEventsFilter(
            query.Page, query.PerPage,
            query.MatchId, query.RosterId,
            query.EventType, query.FavorableTo);

        var paginatedResult = await repository.GetPaginatedAsync(filter, cancellationToken);
        var totalCount      = await repository.CountBySpecificationAsync(filter, includeInactive: true,  cancellationToken);
        var activeCount     = await repository.CountBySpecificationAsync(filter, includeInactive: false, cancellationToken);

        return Result.Success(new AllEventsQueryResult
        {
            Data          = paginatedResult,
            TotalCount    = totalCount,
            ActiveCount   = activeCount,
            InactiveCount = totalCount - activeCount
        });
    }
}

