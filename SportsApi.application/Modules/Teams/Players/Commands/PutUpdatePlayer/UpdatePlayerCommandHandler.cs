using SportsApi.application.Modules.Teams.Players.Filters;
using SportsApi.domain.Abstractions.Auth;
using SportsApi.domain.Abstractions.Dtos;
using SportsApi.domain.Abstractions.Messaging.Commands;
using SportsApi.domain.Abstractions.Persistence;
using SportsApi.domain.Modules.Teams;

namespace SportsApi.application.Modules.Teams.Players.Commands.PutUpdatePlayer;

public class UpdatePlayerCommandHandler(
    IRepository<Player> repository,
    ICoreUnitOfWork unitOfWork,
    ICurrentUser currentUser) : ICommandHandler<UpdatePlayerCommand, UpdatePlayerCommandResult>
{
    public async Task<Result<UpdatePlayerCommandResult>> HandleAsync(
        UpdatePlayerCommand command,
        CancellationToken cancellationToken)
    {
        var entity = await repository.GetBySpecificationAsync(new PlayerByIdFilter(command.Id), cancellationToken);

        if (entity is null)
            return Result.Fail<UpdatePlayerCommandResult>("Player not found", "PLAYER_NOT_FOUND");

        if (command.FullName    is not null) entity.FullName    = command.FullName;
        if (command.Ci          is not null) entity.Ci          = command.Ci;
        if (command.PhoneNumber is not null) entity.PhoneNumber = command.PhoneNumber;
        if (command.IsForeigner is not null) entity.IsForeigner = command.IsForeigner.Value;

        entity.UpdatedAt = DateTime.UtcNow;
        entity.UpdatedBy = currentUser.Username;

        var updateResult = await repository.UpdateAsync(entity, cancellationToken);
        if (updateResult.IsFailure)
            return Result.Fail<UpdatePlayerCommandResult>(updateResult.Error, updateResult.ErrorKey);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(new UpdatePlayerCommandResult { Id = entity.Id });
    }
}

