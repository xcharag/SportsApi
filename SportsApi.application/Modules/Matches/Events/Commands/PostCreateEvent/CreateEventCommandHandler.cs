using SportsApi.domain.Abstractions.Auth;
using SportsApi.domain.Abstractions.Dtos;
using SportsApi.domain.Abstractions.Messaging.Commands;
using SportsApi.domain.Abstractions.Persistence;
using SportsApi.domain.Modules.Matches;

namespace SportsApi.application.Modules.Matches.Events.Commands.PostCreateEvent;

public class CreateEventCommandHandler(
    IRepository<Event> repository,
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

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(new CreateEventCommandResult { Id = entity.Id });
    }
}

