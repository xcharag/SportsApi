using SportsApi.application.Modules.Tournaments.Tournaments.Filters;
using SportsApi.domain.Abstractions.Dtos;
using SportsApi.domain.Abstractions.Messaging.Queries;
using SportsApi.domain.Abstractions.Persistence;
using SportsApi.domain.Enums.Status;
using SportsApi.domain.Modules.Matches;
using SportsApi.domain.Modules.Tournaments;

namespace SportsApi.application.Modules.Tournaments.Tournaments.Queries.GetTournamentStandings;

public class TournamentStandingsQueryHandler(
    IRepository<domain.Modules.Tournaments.RoundsClassified> rcRepository,
    IRepository<Match> matchRepository)
    : IQueryHandler<TournamentStandingsQuery, TournamentStandingsQueryResult>
{
    public async Task<Result<TournamentStandingsQueryResult>> HandleAsync(
        TournamentStandingsQuery query,
        CancellationToken cancellationToken)
    {
        // 1. Load all group-round RoundsClassified (active + inactive teams)
        var rcFilter  = new TournamentGroupRoundsFilter(query.TournamentId, query.GroupKey);
        var allRc     = (await rcRepository.GetListBySpecificationAsync(rcFilter, cancellationToken)).ToList();

        // 2. Load all finished group-stage matches for this tournament
        var matchFilter = new TournamentGroupMatchesFilter(query.TournamentId, finishedOnly: true);
        var matches     = (await matchRepository.GetListBySpecificationAsync(matchFilter, cancellationToken)).ToList();

        // 3. Build a lookup: TeamParticipationId → StandingRow
        var rows = new Dictionary<Guid, StandingRow>();
        foreach (var rc in allRc)
        {
            rows[rc.TeamParticipationId] = new StandingRow
            {
                TeamParticipationId = rc.TeamParticipationId,
                DisplayName         = rc.TeamParticipation!.Name,
                LogoUrl             = rc.TeamParticipation.LogoUrl,
            };
        }

        // 4. Process each finished match
        foreach (var m in matches)
        {
            if (!rows.TryGetValue(m.HomeTeamId, out var homeRow)) continue;
            if (!rows.TryGetValue(m.AwayTeamId, out var awayRow)) continue;

            homeRow.Played++;
            awayRow.Played++;
            homeRow.GoalsFor      += m.ScoreHomeTeam;
            homeRow.GoalsAgainst  += m.ScoreAwayTeam;
            awayRow.GoalsFor      += m.ScoreAwayTeam;
            awayRow.GoalsAgainst  += m.ScoreHomeTeam;

            if (m.ScoreHomeTeam > m.ScoreAwayTeam)
            {
                homeRow.Won++;
                awayRow.Lost++;
            }
            else if (m.ScoreHomeTeam < m.ScoreAwayTeam)
            {
                awayRow.Won++;
                homeRow.Lost++;
            }
            else
            {
                homeRow.Drawn++;
                awayRow.Drawn++;
            }
        }

        // 5. Group rows by their group key (from the RC entries)
        var groupKeys = allRc
            .Select(rc => rc.RoundKey)
            .Distinct()
            .OrderBy(k => k)
            .ToList();

        var rcByTp = allRc.ToDictionary(rc => rc.TeamParticipationId);

        var groups = groupKeys.Select(groupKey =>
        {
            var groupRows = allRc
                .Where(rc => rc.RoundKey == groupKey)
                .Select(rc => rows.TryGetValue(rc.TeamParticipationId, out var row) ? row : null)
                .Where(r => r is not null)
                .Select(r => r!)
                .OrderByDescending(r => r.Points)
                .ThenByDescending(r => r.GoalDifference)
                .ThenByDescending(r => r.GoalsFor)
                .ToList();

            return new GroupStandings { GroupKey = groupKey, Standings = groupRows };
        }).ToList();

        return Result.Success(new TournamentStandingsQueryResult { Groups = groups });
    }
}
