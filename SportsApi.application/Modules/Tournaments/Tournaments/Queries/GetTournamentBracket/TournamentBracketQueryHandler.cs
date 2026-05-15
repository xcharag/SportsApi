using SportsApi.application.Modules.Tournaments.Tournaments.Filters;
using SportsApi.domain.Abstractions.Dtos;
using SportsApi.domain.Abstractions.Messaging.Queries;
using SportsApi.domain.Abstractions.Persistence;
using SportsApi.domain.Enums;
using SportsApi.domain.Enums.Status;
using SportsApi.domain.Modules.Matches;
using SportsApi.domain.Modules.Tournaments;

namespace SportsApi.application.Modules.Tournaments.Tournaments.Queries.GetTournamentBracket;

public class TournamentBracketQueryHandler(
    IRepository<domain.Modules.Tournaments.RoundsClassified> rcRepository,
    IRepository<Match> matchRepository)
    : IQueryHandler<TournamentBracketQuery, TournamentBracketQueryResult>
{
    private static readonly Dictionary<MatchRound, string> RoundNames = new()
    {
        { MatchRound.R16,           "Round of 16"   },
        { MatchRound.QuarterFinals, "Quarter-Finals" },
        { MatchRound.SemiFinals,    "Semi-Finals"    },
        { MatchRound.Final,         "Final"          },
    };

    public async Task<Result<TournamentBracketQueryResult>> HandleAsync(
        TournamentBracketQuery query,
        CancellationToken cancellationToken)
    {
        // Load knockout RCs (including eliminated teams so we can show who lost)
        var rcFilter = new TournamentKnockoutRoundsFilter(query.TournamentId);
        // Use AnyState variant would be ideal but the interface only exposes GetListBySpecification.
        // Instead, RoundsClassified entries where Active=false are already excluded by the global filter.
        // We work only with active entries (teams still in, or already in this round) plus the entries 
        // that were created for this round (winner from prev round is active=true until they lose here).
        var allRc = (await rcRepository.GetListBySpecificationAsync(rcFilter, cancellationToken)).ToList();

        // Load knockout matches
        var matchFilter = new TournamentKnockoutMatchesFilter(query.TournamentId);
        var allMatches  = (await matchRepository.GetListBySpecificationAsync(matchFilter, cancellationToken)).ToList();

        // Build lookup: (homeTPId, awayTPId) → Match
        var matchLookup = new Dictionary<(Guid, Guid), Match>();
        foreach (var m in allMatches)
        {
            matchLookup[(m.HomeTeamId, m.AwayTeamId)] = m;
            matchLookup[(m.AwayTeamId, m.HomeTeamId)] = m; // reverse lookup
        }

        // Group RCs by round
        var rounds = allRc
            .GroupBy(rc => rc.Round)
            .OrderBy(g => g.Key)
            .ToList();

        var bracketRounds = new List<BracketRound>();

        foreach (var roundGroup in rounds)
        {
            var round     = roundGroup.Key;
            var roundName = RoundNames.GetValueOrDefault(round, round.ToString());
            var rcList    = roundGroup.ToList();

            // Group by NextRoundKey to form slots (two teams with the same NextRoundKey play each other).
            // Final round teams may have null NextRoundKey — treat each as its own slot.
            List<BracketSlot> slots;

            if (round == MatchRound.Final)
            {
                // All RC entries in the Final form a single slot
                slots = BuildSlotsFromList(rcList, matchLookup, groupByNext: false);
            }
            else
            {
                slots = BuildSlotsFromList(rcList, matchLookup, groupByNext: true);
            }

            bracketRounds.Add(new BracketRound
            {
                Round     = round,
                RoundName = roundName,
                Slots     = slots,
            });
        }

        return Result.Success(new TournamentBracketQueryResult { Rounds = bracketRounds });
    }

    private static List<BracketSlot> BuildSlotsFromList(
        List<domain.Modules.Tournaments.RoundsClassified> rcList,
        Dictionary<(Guid, Guid), Match> matchLookup,
        bool groupByNext)
    {
        var slots = new List<BracketSlot>();

        if (groupByNext)
        {
            var grouped = rcList.GroupBy(rc => rc.NextRoundKey ?? rc.RoundKey);

            foreach (var slotGroup in grouped.OrderBy(g => g.Key))
            {
                var entries = slotGroup.ToList();
                var slot    = BuildSlot(entries, slotGroup.Key, matchLookup);
                slots.Add(slot);
            }
        }
        else
        {
            // Final: all entries in one slot
            var slot = BuildSlot(rcList, null, matchLookup);
            slots.Add(slot);
        }

        return slots;
    }

    private static BracketSlot BuildSlot(
        List<domain.Modules.Tournaments.RoundsClassified> entries,
        string? winnerAdvancesTo,
        Dictionary<(Guid, Guid), Match> matchLookup)
    {
        var slot = new BracketSlot { WinnerAdvancesTo = winnerAdvancesTo };

        var first  = entries.Count > 0 ? entries[0] : null;
        var second = entries.Count > 1 ? entries[1] : null;

        if (first is not null)
        {
            slot.HomeEntry = new BracketTeamEntry
            {
                TeamParticipationId = first.TeamParticipationId,
                DisplayName         = first.TeamParticipation!.Name,
                LogoUrl             = first.TeamParticipation.LogoUrl,
                RoundKey            = first.RoundKey,
                IsActive            = first.Active,
            };
        }

        if (second is not null)
        {
            slot.AwayEntry = new BracketTeamEntry
            {
                TeamParticipationId = second.TeamParticipationId,
                DisplayName         = second.TeamParticipation!.Name,
                LogoUrl             = second.TeamParticipation.LogoUrl,
                RoundKey            = second.RoundKey,
                IsActive            = second.Active,
            };
        }

        // Try to find the match between these two teams
        if (first is not null && second is not null)
        {
            if (matchLookup.TryGetValue((first.TeamParticipationId, second.TeamParticipationId), out var match))
            {
                slot.Match = new BracketMatchSummary
                {
                    Id        = match.Id,
                    HomeScore = match.ScoreHomeTeam,
                    AwayScore = match.ScoreAwayTeam,
                    Status    = match.Status,
                };

                // Determine winner from finished match
                if (match.Status == MatchStatus.Finished)
                {
                    BracketTeamEntry? winner = null;
                    if (match.ScoreHomeTeam > match.ScoreAwayTeam)
                        winner = match.HomeTeamId == first.TeamParticipationId ? slot.HomeEntry : slot.AwayEntry;
                    else if (match.ScoreAwayTeam > match.ScoreHomeTeam)
                        winner = match.AwayTeamId == first.TeamParticipationId ? slot.HomeEntry : slot.AwayEntry;

                    slot.Winner = winner;
                }
            }
        }

        return slot;
    }
}
