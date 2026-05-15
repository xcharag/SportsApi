using SportsApi.application.Modules.Teams.Players.Filters;
using SportsApi.domain.Abstractions.Dtos;
using SportsApi.domain.Abstractions.Messaging.Queries;
using SportsApi.domain.Abstractions.Persistence;
using SportsApi.domain.Enums;
using SportsApi.domain.Enums.Types;
using SportsApi.domain.Modules.Matches;
using SportsApi.domain.Modules.Teams;

namespace SportsApi.application.Modules.Teams.Players.Queries.GetPlayerStats;

public class PlayerStatsQueryHandler(
    IRepository<Player> playerRepository,
    IRepository<Roster> rosterRepository)
    : IQueryHandler<PlayerStatsQuery, PlayerStatsQueryResult>
{
    public async Task<Result<PlayerStatsQueryResult>> HandleAsync(
        PlayerStatsQuery query,
        CancellationToken cancellationToken)
    {
        var player = await playerRepository.GetBySpecificationAsync(
            new PlayerByIdFilter(query.PlayerId), cancellationToken);

        if (player is null)
            return Result.Fail<PlayerStatsQueryResult>("Player not found", "PLAYER_NOT_FOUND");

        var rosters = await rosterRepository.GetListBySpecificationAsync(
            new PlayerRostersWithEventsFilter(query.PlayerId), cancellationToken);

        var tournamentStats = rosters
            .Where(r => r.Team?.Tournament is not null)
            .Select(r =>
            {
                var events = r.Events ?? [];
                return new PlayerTournamentStats
                {
                    TournamentId        = r.Team.TournamentId,
                    TournamentName      = r.Team.Tournament.Name,
                    TeamParticipationId = r.TeamParticipationId,
                    TeamName            = r.Team.Name,
                    ShirtName           = r.ShirtName,
                    ShirtNumber         = r.ShirtNumber,
                    Stats               = BuildTotals(events),
                };
            })
            .ToList();

        var career = new PlayerStatTotals
        {
            Goals       = tournamentStats.Sum(t => t.Stats.Goals),
            YellowCards = tournamentStats.Sum(t => t.Stats.YellowCards),
            RedCards    = tournamentStats.Sum(t => t.Stats.RedCards),
            Penalties   = tournamentStats.Sum(t => t.Stats.Penalties),
            Offsides    = tournamentStats.Sum(t => t.Stats.Offsides),
            Corners     = tournamentStats.Sum(t => t.Stats.Corners),
            FreeKicks   = tournamentStats.Sum(t => t.Stats.FreeKicks),
        };

        return Result.Success(new PlayerStatsQueryResult
        {
            PlayerId    = player.Id,
            FullName    = player.FullName,
            Career      = career,
            Tournaments = tournamentStats,
        });
    }

    private static PlayerStatTotals BuildTotals(IEnumerable<Event> events)
    {
        var totals = new PlayerStatTotals();
        foreach (var e in events)
        {
            switch (e.EventType)
            {
                case EventType.Goal:      totals.Goals++;       break;
                case EventType.YellowCard: totals.YellowCards++; break;
                case EventType.RedCard:   totals.RedCards++;    break;
                case EventType.Penalty:   totals.Penalties++;   break;
                case EventType.Offside:   totals.Offsides++;    break;
                case EventType.Corner:    totals.Corners++;     break;
                case EventType.FreeKick:  totals.FreeKicks++;   break;
            }
        }
        return totals;
    }
}
