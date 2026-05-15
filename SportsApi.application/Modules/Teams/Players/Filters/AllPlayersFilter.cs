using Ardalis.Specification;
using SportsApi.domain.Abstractions.Specifications;
using SportsApi.domain.Modules.Teams;

namespace SportsApi.application.Modules.Teams.Players.Filters;

public sealed class AllPlayersFilter : PaginationSpecification<Player>
{
    public AllPlayersFilter(int page, int perPage, string? name, bool? isForeigner) : base(page, perPage)
    {
        if (!string.IsNullOrWhiteSpace(name))
        {
            var term = name.ToLower();
            Query.Where(p => p.FullName.ToLower().Contains(term));
        }

        if (isForeigner.HasValue)
            Query.Where(p => p.IsForeigner == isForeigner.Value);

        Query.OrderBy(p => p.FullName);
    }
}


