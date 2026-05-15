using SportsApi.application.Modules.Tournaments.Tournaments.Filters;
using SportsApi.domain.Abstractions.Dtos;
using SportsApi.domain.Abstractions.Messaging.Queries;
using SportsApi.domain.Abstractions.Persistence;
using SportsApi.domain.Modules.Matches;

namespace SportsApi.application.Modules.Tournaments.Tournaments.Queries.GetTournamentTopScorers;

public class TournamentTopScorersQueryHandler(IRepository<Event> eventRepository)
    : IQueryHandler<TournamentTopScorersQuery, TournamentTopScorersQueryResult>
{
    public async Task<Result<TournamentTopScorersQueryResult>> HandleAsync(
        TournamentTopScorersQuery query,
        CancellationToken cancellationToken)
    {
        var goalEvents = (await eventRepository.GetListBySpecificationAsync(
            new TournamentGoalEventsFilter(query.TournamentId), cancellationToken)).ToList();

        var topScorers = goalEvents
            .GroupBy(e => e.RosterId)
            .Select(g =>
            {
                var first = g.First();
                return new TopScorerRow
                {
                    PlayerId            = first.Roster!.PlayerId,
                    PlayerName          = first.Roster.Player.FullName,
                    RosterId            = first.RosterId,
                    TeamParticipationId = first.Roster.TeamParticipationId,
                    TeamName            = first.Roster.Team.Name,
                    ShirtName           = first.Roster.ShirtName,
                    ShirtNumber         = first.Roster.ShirtNumber,
                    Goals               = g.Count(),
                };
            })
            .OrderByDescending(r => r.Goals)
            .Take(query.Limit > 0 ? query.Limit : 10)
            .ToList();

        // Assign rank (players tied on goals share the same rank)
        var rank  = 1;
        var prev  = -1;
        var count = 0;
        foreach (var row in topScorers)
        {
            count++;
            if (row.Goals != prev)
            {
                rank = count;
                prev = row.Goals;
            }
            row.Rank = rank;
        }

        return Result.Success(new TournamentTopScorersQueryResult { TopScorers = topScorers });
    }
}
