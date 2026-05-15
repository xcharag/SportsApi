using System.Text.Json;
using SportsApi.application.Modules.Matches.Matches.Filters;
using SportsApi.domain.Abstractions.Auth;
using SportsApi.domain.Abstractions.Dtos;
using SportsApi.domain.Abstractions.Live;
using SportsApi.domain.Abstractions.Messaging.Commands;
using SportsApi.domain.Abstractions.Persistence;
using SportsApi.domain.Enums;
using SportsApi.domain.Enums.Types;
using SportsApi.domain.Modules.Matches;

namespace SportsApi.application.Modules.Matches.Events.Commands.PostCreateEvent;

public class CreateEventCommandHandler(
    IRepository<Event> repository,
    IRepository<Match> matchRepository,
    IMatchLiveHub liveHub,
    ICoreUnitOfWork unitOfWork,
    ICurrentUser currentUser) : ICommandHandler<CreateEventCommand, CreateEventCommandResult>
{
    public async Task<Result<CreateEventCommandResult>> HandleAsync(
        CreateEventCommand command,
        CancellationToken cancellationToken)
    {
        var entity = new Event
        {
            Minute      = command.Minute,
            FavorableTo = command.FavorableTo,
            EventType   = command.EventType,
            RosterId    = command.RosterId,
            MatchId     = command.MatchId,
            CreatedBy   = currentUser.Username
        };

        var saveResult = await repository.SaveAsync(entity, cancellationToken);
        if (saveResult.IsFailure)
            return Result.Fail<CreateEventCommandResult>(saveResult.Error);

        // Auto-update match score for goal events
        if (command.EventType == EventType.Goal)
        {
            var match = await matchRepository.GetBySpecificationAsync(
                new MatchByIdFilter(command.MatchId), cancellationToken);

            if (match is not null)
            {
                if (command.FavorableTo == FavorableTo.Home)
                    match.ScoreHomeTeam++;
                else
                    match.ScoreAwayTeam++;

                match.UpdatedAt = DateTime.UtcNow;
                match.UpdatedBy = currentUser.Username;

                var matchUpdate = await matchRepository.UpdateAsync(match, cancellationToken);
                if (matchUpdate.IsFailure)
                    return Result.Fail<CreateEventCommandResult>(matchUpdate.Error);

                // Publish live score update
                var scoreUpdate = JsonSerializer.Serialize(new
                {
                    type         = "score",
                    matchId      = match.Id,
                    homeScore    = match.ScoreHomeTeam,
                    awayScore    = match.ScoreAwayTeam,
                    eventId      = entity.Id,
                    eventType    = entity.EventType.ToString(),
                    minute       = entity.Minute,
                    favorableTo  = entity.FavorableTo.ToString(),
                });
                liveHub.Publish(command.MatchId, scoreUpdate);
            }
        }
        else
        {
            // Publish non-goal event too (cards, corners, etc.)
            var eventUpdate = JsonSerializer.Serialize(new
            {
                type        = "event",
                matchId     = command.MatchId,
                eventId     = entity.Id,
                eventType   = entity.EventType.ToString(),
                minute      = entity.Minute,
                favorableTo = entity.FavorableTo.ToString(),
            });
            liveHub.Publish(command.MatchId, eventUpdate);
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(new CreateEventCommandResult { Id = entity.Id });
    }
}

