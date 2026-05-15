using SportsApi.domain.Abstractions.Messaging.Queries;

namespace SportsApi.application.Modules.Matches.Events.Queries.GetEventById;

public class EventByIdQuery : IQuery<EventByIdQueryResult>
{
    public Guid Id { get; set; }
}

