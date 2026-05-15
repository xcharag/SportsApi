using Ardalis.Specification;
using SportsApi.domain.Abstractions.Specifications;
using SportsApi.domain.Enums;
using SportsApi.domain.Enums.Types;
using SportsApi.domain.Modules.Matches;

namespace SportsApi.application.Modules.Matches.Events.Filters;

public sealed class AllEventsFilter : PaginationSpecification<Event>
{
    public AllEventsFilter(
        int page,
        int perPage,
        Guid? matchId,
        Guid? rosterId,
        EventType? eventType,
        FavorableTo? favorableTo) : base(page, perPage)
    {
        if (matchId.HasValue)
            Query.Where(e => e.MatchId == matchId.Value);

        if (rosterId.HasValue)
            Query.Where(e => e.RosterId == rosterId.Value);

        if (eventType.HasValue)
            Query.Where(e => e.EventType == eventType.Value);

        if (favorableTo.HasValue)
            Query.Where(e => e.FavorableTo == favorableTo.Value);

        Query.OrderBy(e => e.Minute);
        Query.Include(e => e.Roster);
    }
}


