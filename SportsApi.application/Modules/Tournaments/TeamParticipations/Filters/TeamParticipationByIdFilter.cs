using Ardalis.Specification;
using SportsApi.domain.Modules.Tournaments;

namespace SportsApi.application.Modules.Tournaments.TeamParticipations.Filters;

public class TeamParticipationByIdFilter : Specification<TeamParticipation>
{
    public TeamParticipationByIdFilter(Guid id)
    {
        Query.Where(tp => tp.Id == id);
    }
}

