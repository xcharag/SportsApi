using Ardalis.Specification;

namespace SportsApi.application.Modules.Tournaments.RoundsClassified.Filters;

public sealed class RoundsClassifiedByIdFilter : Specification<domain.Modules.Tournaments.RoundsClassified>
{
    public RoundsClassifiedByIdFilter(Guid id)
    {
        Query.Where(r => r.Id == id);
        Query.Include(r => r.TeamParticipation!);
    }
}

