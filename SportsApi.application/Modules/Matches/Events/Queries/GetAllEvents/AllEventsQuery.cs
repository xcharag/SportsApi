using SportsApi.domain.Abstractions.Messaging.Queries;
using SportsApi.domain.Enums;
using SportsApi.domain.Enums.Types;

namespace SportsApi.application.Modules.Matches.Events.Queries.GetAllEvents;

public class AllEventsQuery : IQuery<AllEventsQueryResult>
{
    public int          Page        { get; set; } = 1;
    public int          PerPage     { get; set; } = 50;
    public Guid?        MatchId     { get; set; }
    public Guid?        RosterId    { get; set; }
    public EventType?   EventType   { get; set; }
    public FavorableTo? FavorableTo { get; set; }
}

