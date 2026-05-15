using SportsApi.domain.Abstractions.Messaging.Queries;
using SportsApi.domain.Modules.Matches;

namespace SportsApi.application.Modules.Matches.Events.Queries.GetEventById;

public class EventByIdQueryResult : IQueryResult
{
    public required Event Data { get; set; }
}

