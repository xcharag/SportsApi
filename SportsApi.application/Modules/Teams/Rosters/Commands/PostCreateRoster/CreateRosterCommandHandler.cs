using SportsApi.domain.Abstractions.Auth;
using SportsApi.domain.Abstractions.Dtos;
using SportsApi.domain.Abstractions.Messaging.Commands;
using SportsApi.domain.Abstractions.Persistence;
using SportsApi.domain.Modules.Teams;

namespace SportsApi.application.Modules.Teams.Rosters.Commands.PostCreateRoster;

public class CreateRosterCommandHandler(
    IRepository<Roster> repository,
    ICoreUnitOfWork unitOfWork,
    ICurrentUser currentUser) : ICommandHandler<CreateRosterCommand, CreateRosterCommandResult>
{
    public async Task<Result<CreateRosterCommandResult>> HandleAsync(
        CreateRosterCommand command,
        CancellationToken cancellationToken)
    {
        var entity = new Roster
        {
            ShirtNumber         = command.ShirtNumber,
            ShirtName           = command.ShirtName,
            PlayerId            = command.PlayerId,
            TeamParticipationId = command.TeamParticipationId,
            CreatedBy           = currentUser.Username
        };

        var saveResult = await repository.SaveAsync(entity, cancellationToken);
        if (saveResult.IsFailure)
            return Result.Fail<CreateRosterCommandResult>(saveResult.Error);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(new CreateRosterCommandResult { Id = entity.Id });
    }
}

