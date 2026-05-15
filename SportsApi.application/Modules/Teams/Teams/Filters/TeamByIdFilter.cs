using Ardalis.Specification;
using SportsApi.domain.Modules.Teams;

namespace SportsApi.application.Modules.Teams.Teams.Filters;

public class TeamByIdFilter : Specification<Team>
{
    public TeamByIdFilter(Guid id)
    {
        Query.Where(t => t.Id == id);
    }
}

