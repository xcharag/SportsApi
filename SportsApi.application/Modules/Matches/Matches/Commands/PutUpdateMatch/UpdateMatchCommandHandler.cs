using System.Text.Json;
using SportsApi.application.Modules.Matches.Matches.Filters;
using SportsApi.application.Modules.Tournaments.RoundsClassified.Filters;
using SportsApi.domain.Abstractions.Auth;
using SportsApi.domain.Abstractions.Dtos;
using SportsApi.domain.Abstractions.Live;
using SportsApi.domain.Abstractions.Messaging.Commands;
using SportsApi.domain.Abstractions.Persistence;
using SportsApi.domain.Enums;
using SportsApi.domain.Enums.Status;
using SportsApi.domain.Modules.Matches;
using SportsApi.domain.Modules.Tournaments;

namespace SportsApi.application.Modules.Matches.Matches.Commands.PutUpdateMatch;

public class UpdateMatchCommandHandler(
    IRepository<Match> repository,
    IRepository<RoundsClassified> rcRepository,
    IMatchLiveHub liveHub,
    ICoreUnitOfWork unitOfWork,
    ICurrentUser currentUser) : ICommandHandler<UpdateMatchCommand, UpdateMatchCommandResult>
{
    private static readonly MatchRound[] KnockoutRounds =
        [MatchRound.R16, MatchRound.QuarterFinals, MatchRound.SemiFinals, MatchRound.Final];

    private static readonly Dictionary<MatchRound, MatchRound> NextRound = new()
    {
        { MatchRound.R16,           MatchRound.QuarterFinals },
        { MatchRound.QuarterFinals, MatchRound.SemiFinals    },
        { MatchRound.SemiFinals,    MatchRound.Final         },
    };

    public async Task<Result<UpdateMatchCommandResult>> HandleAsync(
        UpdateMatchCommand command,
        CancellationToken cancellationToken)
    {
        var entity = await repository.GetBySpecificationAsync(new MatchByIdFilter(command.Id), cancellationToken);

        if (entity is null)
            return Result.Fail<UpdateMatchCommandResult>("Match not found", "MATCH_NOT_FOUND");

        var wasAlreadyFinished = entity.Status == MatchStatus.Finished;

        if (command.MatchDay      is not null) entity.MatchDay      = command.MatchDay.Value;
        if (command.MatchDate     is not null) entity.MatchDate     = command.MatchDate.Value;
        if (command.Field         is not null) entity.Field         = command.Field;
        if (command.Location      is not null) entity.Location      = command.Location;
        if (command.Status        is not null) entity.Status        = command.Status.Value;
        if (command.ScoreHomeTeam is not null) entity.ScoreHomeTeam = command.ScoreHomeTeam.Value;
        if (command.ScoreAwayTeam is not null) entity.ScoreAwayTeam = command.ScoreAwayTeam.Value;
        if (command.NewMatchId    is not null) entity.NewMatchId    = command.NewMatchId;

        entity.UpdatedAt = DateTime.UtcNow;
        entity.UpdatedBy = currentUser.Username;

        var updateResult = await repository.UpdateAsync(entity, cancellationToken);
        if (updateResult.IsFailure)
            return Result.Fail<UpdateMatchCommandResult>(updateResult.Error, updateResult.ErrorKey);

        // Publish live status/score update
        var statusUpdate = JsonSerializer.Serialize(new
        {
            type      = "status",
            matchId   = entity.Id,
            status    = entity.Status.ToString(),
            homeScore = entity.ScoreHomeTeam,
            awayScore = entity.ScoreAwayTeam,
        });
        liveHub.Publish(entity.Id, statusUpdate);

        // ── Auto-advance for knockout rounds ──────────────────────────────────
        if (!wasAlreadyFinished
            && entity.Status == MatchStatus.Finished
            && KnockoutRounds.Contains(entity.Round))
        {
            var advanceResult = await AdvanceTeamAsync(entity, command.ManualWinnerId, cancellationToken);
            if (advanceResult.IsFailure)
                return Result.Fail<UpdateMatchCommandResult>(advanceResult.Error, advanceResult.ErrorKey);
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(new UpdateMatchCommandResult { Id = entity.Id });
    }

    private async Task<Result> AdvanceTeamAsync(Match match, Guid? manualWinnerId, CancellationToken ct)
    {
        var username = currentUser.Username;

        var homeRc = await rcRepository.GetBySpecificationAsync(
            new RoundsClassifiedByTeamAndRoundFilter(match.HomeTeamId, match.Round), ct);
        var awayRc = await rcRepository.GetBySpecificationAsync(
            new RoundsClassifiedByTeamAndRoundFilter(match.AwayTeamId, match.Round), ct);

        if (homeRc is null || awayRc is null)
            return Result.Success(); // RC missing — skip silently

        Guid winnerId;
        Guid loserId;

        if (manualWinnerId.HasValue)
        {
            if (manualWinnerId.Value != match.HomeTeamId && manualWinnerId.Value != match.AwayTeamId)
                return Result.Fail("ManualWinnerId must be HomeTeamId or AwayTeamId of this match.",
                    "INVALID_MANUAL_WINNER");

            winnerId = manualWinnerId.Value;
            loserId  = winnerId == match.HomeTeamId ? match.AwayTeamId : match.HomeTeamId;
        }
        else
        {
            if (match.ScoreHomeTeam == match.ScoreAwayTeam)
                return Result.Fail(
                    "Match ended in a draw. Provide ManualWinnerId to declare the winner (e.g. after penalties).",
                    "DRAW_REQUIRES_MANUAL_WINNER");

            winnerId = match.ScoreHomeTeam > match.ScoreAwayTeam ? match.HomeTeamId : match.AwayTeamId;
            loserId  = winnerId == match.HomeTeamId ? match.AwayTeamId : match.HomeTeamId;
        }

        var winnerRc = winnerId == match.HomeTeamId ? homeRc : awayRc;
        var loserRc  = loserId  == match.HomeTeamId ? homeRc : awayRc;

        loserRc.Active    = false;
        loserRc.DeletedAt = DateTime.UtcNow;
        loserRc.DeletedBy = username;
        var loserUpdate = await rcRepository.UpdateAsync(loserRc, ct);
        if (loserUpdate.IsFailure)
            return loserUpdate;

        // Advance winner to next round (Final has no next round after it)
        if (NextRound.TryGetValue(match.Round, out var nextRound) && winnerRc.NextRoundKey is not null)
        {
            var newRc = new RoundsClassified
            {
                Id                  = Guid.NewGuid(),
                TeamParticipationId = winnerRc.TeamParticipationId,
                Round               = nextRound,
                RoundKey            = winnerRc.NextRoundKey,
                CreatedBy           = username,
                CreatedAt           = DateTime.UtcNow,
            };

            var saveResult = await rcRepository.SaveAsync(newRc, ct);
            if (saveResult.IsFailure)
                return saveResult;
        }

        return Result.Success();
    }
}

