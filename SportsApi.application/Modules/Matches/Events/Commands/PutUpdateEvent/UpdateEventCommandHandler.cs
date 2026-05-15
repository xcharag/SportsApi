using SportsApi.application.Modules.Matches.Events.Filters;
using SportsApi.domain.Abstractions.Auth;
using SportsApi.domain.Abstractions.Dtos;
using SportsApi.domain.Abstractions.Messaging.Commands;
using SportsApi.domain.Abstractions.Persistence;
using SportsApi.domain.Modules.Matches;

namespace SportsApi.application.Modules.Matches.Events.Commands.PutUpdateEvent;

public class UpdateEventCommandHandler(
    IRepository<Event> repository,
    ICoreUnitOfWork unitOfWork,
    ICurrentUser currentUser) : ICommandHandler<UpdateEventCommand, UpdateEventCommandResult>
{
    public async Task<Result<UpdateEventCommandResult>> HandleAsync(
        UpdateEventCommand command,
        CancellationToken cancellationToken)
    {
        var entity = await repository.GetBySpecificationAsync(new EventByIdFilter(command.Id), cancellationToken);

        if (entity is null)
            return Result.Fail<UpdateEventCommandResult>("Event not found", "EVENT_NOT_FOUND");

        if (command.Minute      is not null) entity.Minute      = command.Minute.Value;
        if (command.FavorableTo is not null) entity.FavorableTo = command.FavorableTo.Value;
        if (command.EventType   is not null) entity.EventType   = command.EventType.Value;

        entity.UpdatedAt = DateTime.Now;
        entity.UpdatedBy = currentUser.Username;

        var updateResult = await repository.UpdateAsync(entity, cancellationToken);
        if (updateResult.IsFailure)
            return Result.Fail<UpdateEventCommandResult>(updateResult.Error, updateResult.ErrorKey);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(new UpdateEventCommandResult { Id = entity.Id });
    }
}

