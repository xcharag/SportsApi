using SportsApi.application.Modules.Teams.Players.Filters;
using SportsApi.domain.Abstractions.Dtos;
using SportsApi.domain.Abstractions.Messaging.Queries;
using SportsApi.domain.Abstractions.Persistence;
using SportsApi.domain.Enums;
using SportsApi.domain.Enums.Types;
using SportsApi.domain.Modules.Teams;

namespace SportsApi.application.Modules.Teams.Players.Queries.GetPlayerProfile;

public class PlayerProfileQueryHandler(
    IRepository<Player> playerRepository,
    IRepository<Roster> rosterRepository)
    : IQueryHandler<PlayerProfileQuery, PlayerProfileQueryResult>
{
    public async Task<Result<PlayerProfileQueryResult>> HandleAsync(
        PlayerProfileQuery query,
        CancellationToken cancellationToken)
    {
        var player = await playerRepository.GetBySpecificationAsync(
            new PlayerByIdFilter(query.PlayerId), cancellationToken);

        if (player is null)
            return Result.Fail<PlayerProfileQueryResult>("Player not found", "PLAYER_NOT_FOUND");

        var rosters = (await rosterRepository.GetListBySpecificationAsync(
            new PlayerRostersWithEventsFilter(query.PlayerId), cancellationToken)).ToList();

        var teams = rosters
            .Where(r => r.Team?.Tournament is not null)
            .Select(r =>
            {
                var events = (r.Events ?? []).Select(e => new PlayerEventRecord
                {
                    EventId   = e.Id,
                    EventType = e.EventType,
                    Minute    = e.Minute,
                    MatchId   = e.MatchId,
                }).ToList();

                return new PlayerTeamEntry
                {
                    TeamParticipationId = r.TeamParticipationId,
                    TeamName            = r.Team.Name,
                    LogoUrl             = r.Team.LogoUrl,
                    TournamentId        = r.Team.TournamentId,
                    TournamentName      = r.Team.Tournament.Name,
                    ShirtName           = r.ShirtName,
                    ShirtNumber         = r.ShirtNumber,
                    Events              = events,
                };
            })
            .ToList();

        var allEvents = rosters.SelectMany(r => r.Events ?? []).ToList();

        var career = new PlayerCareerStats
        {
            Goals       = allEvents.Count(e => e.EventType == EventType.Goal),
            YellowCards = allEvents.Count(e => e.EventType == EventType.YellowCard),
            RedCards    = allEvents.Count(e => e.EventType == EventType.RedCard),
            Penalties   = allEvents.Count(e => e.EventType == EventType.Penalty),
            Offsides    = allEvents.Count(e => e.EventType == EventType.Offside),
            Corners     = allEvents.Count(e => e.EventType == EventType.Corner),
            FreeKicks   = allEvents.Count(e => e.EventType == EventType.FreeKick),
        };

        return Result.Success(new PlayerProfileQueryResult
        {
            PlayerId    = player.Id,
            FullName    = player.FullName,
            Ci          = player.Ci,
            PhoneNumber = player.PhoneNumber,
            IsForeigner = player.IsForeigner,
            Teams       = teams,
            Career      = career,
        });
    }
}
