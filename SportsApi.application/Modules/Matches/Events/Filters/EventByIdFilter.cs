using Ardalis.Specification;
using SportsApi.domain.Modules.Matches;

namespace SportsApi.application.Modules.Matches.Events.Filters;

public class EventByIdFilter : Specification<Event>
{
    public EventByIdFilter(Guid id)
    {
        Query.Where(e => e.Id == id);
        Query.Include(e => e.Roster);
        Query.Include(e => e.Match);
    }
}

