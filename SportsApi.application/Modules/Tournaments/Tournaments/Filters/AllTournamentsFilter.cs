using Ardalis.Specification;
using SportsApi.domain.Abstractions.Specifications;
using SportsApi.domain.Modules.Tournaments;

namespace SportsApi.application.Modules.Tournaments.Tournaments.Filters;

public sealed class AllTournamentsFilter : PaginationSpecification<Tournament>
{
    public AllTournamentsFilter(
        int page,
        int perPage,
        string? name,
        DateTime? initDate,
        DateTime? endDate) : base(page, perPage)
    {
        if (!string.IsNullOrWhiteSpace(name))
        {
            var term = name.ToLower();
            Query.Where(t => t.Name.Contains(term, StringComparison.CurrentCultureIgnoreCase));
        }
        
        if (initDate.HasValue)
            Query.Where(t => t.StartDate >= initDate.Value);
        if (endDate.HasValue)            
            Query.Where(t => t.EndDate <= endDate.Value);
        
        Query.OrderBy(t => t.StartDate);       
        Query.Include(t => t.TeamsParticipations);
    }
}