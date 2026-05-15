using Ardalis.Specification;
using SportsApi.domain.Modules.Teams;

namespace SportsApi.application.Modules.Teams.Players.Filters;

public class PlayerByIdFilter : Specification<Player>
{
    public PlayerByIdFilter(Guid id)
    {
        Query.Where(p => p.Id == id);
    }
}

