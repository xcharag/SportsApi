using SportsApi.application.Modules.Teams.Rosters.Filters;
using SportsApi.domain.Abstractions.Auth;
using SportsApi.domain.Abstractions.Dtos;
using SportsApi.domain.Abstractions.Messaging.Commands;
using SportsApi.domain.Abstractions.Persistence;
using SportsApi.domain.Modules.Teams;

namespace SportsApi.application.Modules.Teams.Rosters.Commands.DeleteRoster;

public class DeleteRosterCommandHandler(
    IRepository<Roster> repository,
    ICoreUnitOfWork unitOfWork,
    ICurrentUser currentUser) : ICommandHandler<DeleteRosterCommand, DeleteRosterCommandResult>
{
    public async Task<Result<DeleteRosterCommandResult>> HandleAsync(
        DeleteRosterCommand command,
        CancellationToken cancellationToken)
    {
        var entity = await repository.GetBySpecificationAsync(new RosterByIdFilter(command.Id), cancellationToken);

        if (entity is null)
            return Result.Fail<DeleteRosterCommandResult>("Roster not found", "ROSTER_NOT_FOUND");

        var username = currentUser.Username;

        if (command.HardDelete)
        {
            entity.DeletedAt = DateTime.Now;
            entity.DeletedBy = username;
            var deleteResult = await repository.DeleteAsync(entity, cancellationToken);
            if (deleteResult.IsFailure)
                return Result.Fail<DeleteRosterCommandResult>(deleteResult.Error, deleteResult.ErrorKey);
        }
        else
        {
            entity.Active    = false;
            entity.DeletedAt = DateTime.Now;
            entity.DeletedBy = username;
            var updateResult = await repository.UpdateAsync(entity, cancellationToken);
            if (updateResult.IsFailure)
                return Result.Fail<DeleteRosterCommandResult>(updateResult.Error, updateResult.ErrorKey);
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(new DeleteRosterCommandResult { Id = entity.Id });
    }
}

