using Ardalis.Specification;
using SportsApi.domain.Modules.Teams;

namespace SportsApi.application.Modules.Teams.Teams.Filters;

/// <summary>Gets a team with all its participations, tournaments, rosters, and roster events — for profile display.</summary>
public sealed class TeamProfileFilter : Specification<Team>
{
    public TeamProfileFilter(Guid teamId)
    {
        Query.Where(t => t.Id == teamId);

        Query.Include(t => t.TeamParticipations!)
            .ThenInclude(tp => tp.Tournament);
        Query.Include(t => t.TeamParticipations!)
            .ThenInclude(tp => tp.Rosters!)
            .ThenInclude(r => r.Events!);
        Query.Include(t => t.TeamParticipations!)
            .ThenInclude(tp => tp.Rosters!)
            .ThenInclude(r => r.Player);
    }
}
