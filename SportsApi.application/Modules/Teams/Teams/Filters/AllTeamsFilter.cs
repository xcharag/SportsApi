using Ardalis.Specification;
using SportsApi.domain.Abstractions.Specifications;
using SportsApi.domain.Modules.Teams;

namespace SportsApi.application.Modules.Teams.Teams.Filters;

public sealed class AllTeamsFilter : PaginationSpecification<Team>
{
    public AllTeamsFilter(int page, int perPage, string? name) : base(page, perPage)
    {
        if (!string.IsNullOrWhiteSpace(name))
        {
            var term = name.ToLower();
            Query.Where(t => t.DefaultName.ToLower().Contains(term));
        }

        Query.OrderBy(t => t.DefaultName);
        Query.Include(t => t.TeamParticipations!);
    }
}


