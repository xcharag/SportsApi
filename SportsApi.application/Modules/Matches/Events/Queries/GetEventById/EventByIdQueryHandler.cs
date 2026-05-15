using SportsApi.application.Modules.Matches.Events.Filters;
using SportsApi.domain.Abstractions.Dtos;
using SportsApi.domain.Abstractions.Messaging.Queries;
using SportsApi.domain.Abstractions.Persistence;
using SportsApi.domain.Modules.Matches;

namespace SportsApi.application.Modules.Matches.Events.Queries.GetEventById;

public class EventByIdQueryHandler(IRepository<Event> repository)
    : IQueryHandler<EventByIdQuery, EventByIdQueryResult>
{
    public async Task<Result<EventByIdQueryResult>> HandleAsync(
        EventByIdQuery query,
        CancellationToken cancellationToken)
    {
        var entity = await repository.GetBySpecificationAsync(new EventByIdFilter(query.Id), cancellationToken);

        if (entity is null)
            return Result.Fail<EventByIdQueryResult>("Event not found", "EVENT_NOT_FOUND");

        return Result.Success(new EventByIdQueryResult { Data = entity });
    }
}

