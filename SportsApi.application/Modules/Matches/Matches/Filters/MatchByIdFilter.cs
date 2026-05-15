using Ardalis.Specification;
using SportsApi.domain.Modules.Matches;

namespace SportsApi.application.Modules.Matches.Matches.Filters;

public class MatchByIdFilter : Specification<Match>
{
    public MatchByIdFilter(Guid id)
    {
        Query.Where(m => m.Id == id);
        Query.Include(m => m.HomeTeam);
        Query.Include(m => m.AwayTeam);
    }
}

