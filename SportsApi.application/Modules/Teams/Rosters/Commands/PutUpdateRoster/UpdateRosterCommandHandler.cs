using SportsApi.application.Modules.Teams.Rosters.Filters;
using SportsApi.domain.Abstractions.Auth;
using SportsApi.domain.Abstractions.Dtos;
using SportsApi.domain.Abstractions.Messaging.Commands;
using SportsApi.domain.Abstractions.Persistence;
using SportsApi.domain.Modules.Teams;

namespace SportsApi.application.Modules.Teams.Rosters.Commands.PutUpdateRoster;

public class UpdateRosterCommandHandler(
    IRepository<Roster> repository,
    ICoreUnitOfWork unitOfWork,
    ICurrentUser currentUser) : ICommandHandler<UpdateRosterCommand, UpdateRosterCommandResult>
{
    public async Task<Result<UpdateRosterCommandResult>> HandleAsync(
        UpdateRosterCommand command,
        CancellationToken cancellationToken)
    {
        var entity = await repository.GetBySpecificationAsync(new RosterByIdFilter(command.Id), cancellationToken);

        if (entity is null)
            return Result.Fail<UpdateRosterCommandResult>("Roster not found", "ROSTER_NOT_FOUND");

        if (command.ShirtNumber is not null) entity.ShirtNumber = command.ShirtNumber;
        if (command.ShirtName   is not null) entity.ShirtName   = command.ShirtName;

        entity.UpdatedAt = DateTime.Now;
        entity.UpdatedBy = currentUser.Username;

        var updateResult = await repository.UpdateAsync(entity, cancellationToken);
        if (updateResult.IsFailure)
            return Result.Fail<UpdateRosterCommandResult>(updateResult.Error, updateResult.ErrorKey);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(new UpdateRosterCommandResult { Id = entity.Id });
    }
}

