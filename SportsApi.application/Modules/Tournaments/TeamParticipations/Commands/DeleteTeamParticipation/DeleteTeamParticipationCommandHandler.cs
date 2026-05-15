using SportsApi.application.Modules.Tournaments.TeamParticipations.Filters;
using SportsApi.domain.Abstractions.Auth;
using SportsApi.domain.Abstractions.Dtos;
using SportsApi.domain.Abstractions.Messaging.Commands;
using SportsApi.domain.Abstractions.Persistence;
using SportsApi.domain.Modules.Tournaments;

namespace SportsApi.application.Modules.Tournaments.TeamParticipations.Commands.DeleteTeamParticipation;

public class DeleteTeamParticipationCommandHandler(
    IRepository<TeamParticipation> repository,
    ICoreUnitOfWork unitOfWork,
    ICurrentUser currentUser) : ICommandHandler<DeleteTeamParticipationCommand, DeleteTeamParticipationCommandResult>
{
    public async Task<Result<DeleteTeamParticipationCommandResult>> HandleAsync(
        DeleteTeamParticipationCommand command,
        CancellationToken cancellationToken)
    {
        var entity = await repository.GetBySpecificationAsync(
            new TeamParticipationByIdFilter(command.Id), cancellationToken);

        if (entity is null)
            return Result.Fail<DeleteTeamParticipationCommandResult>("TeamParticipation not found", "TEAM_PARTICIPATION_NOT_FOUND");

        var username = currentUser.Username;

        if (command.HardDelete)
        {
            entity.DeletedAt = DateTime.Now;
            entity.DeletedBy = username;
            var deleteResult = await repository.DeleteAsync(entity, cancellationToken);
            if (deleteResult.IsFailure)
                return Result.Fail<DeleteTeamParticipationCommandResult>(deleteResult.Error, deleteResult.ErrorKey);
        }
        else
        {
            entity.Active    = false;
            entity.DeletedAt = DateTime.Now;
            entity.DeletedBy = username;
            var updateResult = await repository.UpdateAsync(entity, cancellationToken);
            if (updateResult.IsFailure)
                return Result.Fail<DeleteTeamParticipationCommandResult>(updateResult.Error, updateResult.ErrorKey);
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(new DeleteTeamParticipationCommandResult { Id = entity.Id });
    }
}

