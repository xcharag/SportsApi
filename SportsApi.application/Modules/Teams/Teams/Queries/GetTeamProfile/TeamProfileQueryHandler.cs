using SportsApi.application.Modules.Teams.Teams.Filters;
using SportsApi.application.Modules.Tournaments.RoundsClassified.Filters;
using SportsApi.domain.Abstractions.Dtos;
using SportsApi.domain.Abstractions.Messaging.Queries;
using SportsApi.domain.Abstractions.Persistence;
using SportsApi.domain.Enums;
using SportsApi.domain.Enums.Types;
using SportsApi.domain.Modules.Teams;

namespace SportsApi.application.Modules.Teams.Teams.Queries.GetTeamProfile;

public class TeamProfileQueryHandler(
    IRepository<Team> teamRepository,
    IRepository<domain.Modules.Tournaments.RoundsClassified> rcRepository)
    : IQueryHandler<TeamProfileQuery, TeamProfileQueryResult>
{
    public async Task<Result<TeamProfileQueryResult>> HandleAsync(
        TeamProfileQuery query,
        CancellationToken cancellationToken)
    {
        var team = await teamRepository.GetBySpecificationAsync(
            new TeamProfileFilter(query.TeamId), cancellationToken);

        if (team is null)
            return Result.Fail<TeamProfileQueryResult>("Team not found", "TEAM_NOT_FOUND");

        // Get Final-round RCs to mark championships (active = team won that tournament)
        var finalRcs = (await rcRepository.GetListBySpecificationAsync(
            new FinalRoundsForTeamFilter(query.TeamId), cancellationToken)).ToList();

        var championTpIds = finalRcs
            .Where(r => r.Active)
            .Select(r => r.TeamParticipationId)
            .ToHashSet();

        var history = (team.TeamParticipations ?? [])
            .OrderByDescending(tp => tp.Tournament.StartDate)
            .Select(tp => new TeamTournamentHistory
            {
                TournamentId        = tp.TournamentId,
                TournamentName      = tp.Tournament.Name,
                TeamParticipationId = tp.Id,
                ParticipationName   = tp.Name,
                LogoUrl             = tp.LogoUrl,
                IsChampion          = championTpIds.Contains(tp.Id),
            })
            .ToList();

        // Aggregate career stats from all roster events
        var allEvents = (team.TeamParticipations ?? [])
            .SelectMany(tp => tp.Rosters ?? [])
            .SelectMany(r => r.Events ?? [])
            .ToList();

        var careerStats = new TeamCareerStats
        {
            Goals       = allEvents.Count(e => e.EventType == EventType.Goal),
            YellowCards = allEvents.Count(e => e.EventType == EventType.YellowCard),
            RedCards    = allEvents.Count(e => e.EventType == EventType.RedCard),
            Penalties   = allEvents.Count(e => e.EventType == EventType.Penalty),
        };

        // Historic top scorers
        var topScorers = (team.TeamParticipations ?? [])
            .SelectMany(tp => tp.Rosters ?? [])
            .GroupBy(r => r.PlayerId)
            .Select(g => new TeamTopScorerRow
            {
                PlayerId   = g.Key,
                PlayerName = g.First().Player.FullName,
                TotalGoals = g.Sum(r => (r.Events ?? []).Count(e => e.EventType == EventType.Goal)),
            })
            .Where(r => r.TotalGoals > 0)
            .OrderByDescending(r => r.TotalGoals)
            .Take(10)
            .ToList();

        return Result.Success(new TeamProfileQueryResult
        {
            TeamId            = team.Id,
            DefaultName       = team.DefaultName,
            DefaultLogoUrl    = team.DefaultLogoUrl,
            TournamentHistory = history,
            TopScorers        = topScorers,
            CareerStats       = careerStats,
        });
    }
}
