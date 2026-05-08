using Ardalis.Specification;
using SportsApi.domain.Modules.Tournaments;

namespace SportsApi.application.Modules.Tournaments.Tournaments.Filters;

public class TournamentByIdFilter : Specification<Tournament>
{
    public TournamentByIdFilter(Guid id)
    {
        Query.Where(t => t.Id == id);
    }
}