using Ardalis.Specification;
using SportsApi.domain.Abstractions.Specifications;
using SportsApi.domain.Modules.Tournaments;

namespace SportsApi.application.Modules.Tournaments.TeamParticipations.Filters;

public sealed class AllTeamParticipationsFilter : PaginationSpecification<TeamParticipation>
{
    public AllTeamParticipationsFilter(
        int page,
        int perPage,
        Guid? tournamentId,
        Guid? teamId,
        string? name) : base(page, perPage)
    {
        if (tournamentId.HasValue)
            Query.Where(tp => tp.TournamentId == tournamentId.Value);

        if (teamId.HasValue)
            Query.Where(tp => tp.TeamId == teamId.Value);

        if (!string.IsNullOrWhiteSpace(name))
        {
            var term = name.ToLower();
            Query.Where(tp => tp.Name.ToLower().Contains(term));
        }

        Query.OrderBy(tp => tp.Name);
        Query.Include(tp => tp.Team);
        Query.Include(tp => tp.Tournament);
    }
}


