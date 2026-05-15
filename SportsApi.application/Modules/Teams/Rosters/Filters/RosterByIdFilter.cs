using Ardalis.Specification;
using SportsApi.domain.Modules.Teams;

namespace SportsApi.application.Modules.Teams.Rosters.Filters;

public class RosterByIdFilter : Specification<Roster>
{
    public RosterByIdFilter(Guid id)
    {
        Query.Where(r => r.Id == id);
    }
}

