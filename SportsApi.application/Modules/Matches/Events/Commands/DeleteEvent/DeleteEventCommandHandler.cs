using SportsApi.application.Modules.Matches.Events.Filters;
using SportsApi.domain.Abstractions.Auth;
using SportsApi.domain.Abstractions.Dtos;
using SportsApi.domain.Abstractions.Messaging.Commands;
using SportsApi.domain.Abstractions.Persistence;
using SportsApi.domain.Modules.Matches;

namespace SportsApi.application.Modules.Matches.Events.Commands.DeleteEvent;

public class DeleteEventCommandHandler(
    IRepository<Event> repository,
    ICoreUnitOfWork unitOfWork,
    ICurrentUser currentUser) : ICommandHandler<DeleteEventCommand, DeleteEventCommandResult>
{
    public async Task<Result<DeleteEventCommandResult>> HandleAsync(
        DeleteEventCommand command,
        CancellationToken cancellationToken)
    {
        var entity = await repository.GetBySpecificationAsync(new EventByIdFilter(command.Id), cancellationToken);

        if (entity is null)
            return Result.Fail<DeleteEventCommandResult>("Event not found", "EVENT_NOT_FOUND");

        var username = currentUser.Username;

        if (command.HardDelete)
        {
            entity.DeletedAt = DateTime.UtcNow;
            entity.DeletedBy = username;
            var deleteResult = await repository.DeleteAsync(entity, cancellationToken);
            if (deleteResult.IsFailure)
                return Result.Fail<DeleteEventCommandResult>(deleteResult.Error, deleteResult.ErrorKey);
        }
        else
        {
            entity.Active    = false;
            entity.DeletedAt = DateTime.UtcNow;
            entity.DeletedBy = username;
            var updateResult = await repository.UpdateAsync(entity, cancellationToken);
            if (updateResult.IsFailure)
                return Result.Fail<DeleteEventCommandResult>(updateResult.Error, updateResult.ErrorKey);
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(new DeleteEventCommandResult { Id = entity.Id });
    }
}

