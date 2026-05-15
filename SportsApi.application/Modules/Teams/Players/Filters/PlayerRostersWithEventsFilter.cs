using Ardalis.Specification;
using SportsApi.domain.Modules.Teams;

namespace SportsApi.application.Modules.Teams.Players.Filters;

/// <summary>Gets all Roster entries for a player, including their team participation, tournament, and all events.</summary>
public sealed class PlayerRostersWithEventsFilter : Specification<Roster>
{
    public PlayerRostersWithEventsFilter(Guid playerId)
    {
        Query.Where(r => r.PlayerId == playerId);

        Query.Include(r => r.Team)
            .ThenInclude(tp => tp.Tournament);
        Query.Include(r => r.Events!);
        Query.Include(r => r.Player);
    }
}
