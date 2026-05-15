using SportsApi.domain.Abstractions.Auth;
using SportsApi.domain.Abstractions.Dtos;
using SportsApi.domain.Abstractions.Messaging.Commands;
using SportsApi.domain.Abstractions.Persistence;
using SportsApi.domain.Modules.Teams;

namespace SportsApi.application.Modules.Teams.Players.Commands.PostCreatePlayer;

public class CreatePlayerCommandHandler(
    IRepository<Player> repository,
    ICoreUnitOfWork unitOfWork,
    ICurrentUser currentUser) : ICommandHandler<CreatePlayerCommand, CreatePlayerCommandResult>
{
    public async Task<Result<CreatePlayerCommandResult>> HandleAsync(
        CreatePlayerCommand command,
        CancellationToken cancellationToken)
    {
        var entity = new Player
        {
            FullName    = command.FullName,
            Ci          = command.Ci,
            PhoneNumber = command.PhoneNumber,
            IsForeigner = command.IsForeigner,
            CreatedBy   = currentUser.Username
        };

        var saveResult = await repository.SaveAsync(entity, cancellationToken);
        if (saveResult.IsFailure)
            return Result.Fail<CreatePlayerCommandResult>(saveResult.Error);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(new CreatePlayerCommandResult { Id = entity.Id, FullName = entity.FullName });
    }
}

