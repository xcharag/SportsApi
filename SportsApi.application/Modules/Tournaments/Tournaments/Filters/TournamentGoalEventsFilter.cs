using Ardalis.Specification;
using SportsApi.domain.Enums;
using SportsApi.domain.Enums.Types;
using SportsApi.domain.Modules.Matches;

namespace SportsApi.application.Modules.Tournaments.Tournaments.Filters;

/// <summary>Gets all Goal events for a tournament by joining through Roster → TeamParticipation.</summary>
public sealed class TournamentGoalEventsFilter : Specification<Event>
{
    public TournamentGoalEventsFilter(Guid tournamentId)
    {
        Query.Where(e => e.EventType == EventType.Goal
                         && e.Roster!.Team.TournamentId == tournamentId);

        Query.Include(e => e.Roster!)
            .ThenInclude(r => r.Player);
        Query.Include(e => e.Roster!)
            .ThenInclude(r => r.Team);
    }
}
