using SportsApi.application.Modules.Teams.Teams.Filters;
using SportsApi.domain.Abstractions.Auth;
using SportsApi.domain.Abstractions.Dtos;
using SportsApi.domain.Abstractions.Messaging.Commands;
using SportsApi.domain.Abstractions.Persistence;
using SportsApi.domain.Modules.Teams;

namespace SportsApi.application.Modules.Teams.Teams.Commands.DeleteTeam;

public class DeleteTeamCommandHandler(
    IRepository<Team> repository,
    ICoreUnitOfWork unitOfWork,
    ICurrentUser currentUser) : ICommandHandler<DeleteTeamCommand, DeleteTeamCommandResult>
{
    public async Task<Result<DeleteTeamCommandResult>> HandleAsync(
        DeleteTeamCommand command,
        CancellationToken cancellationToken)
    {
        var entity = await repository.GetBySpecificationAsync(new TeamByIdFilter(command.Id), cancellationToken);

        if (entity is null)
            return Result.Fail<DeleteTeamCommandResult>("Team not found", "TEAM_NOT_FOUND");

        var username = currentUser.Username;

        if (command.HardDelete)
        {
            entity.DeletedAt = DateTime.Now;
            entity.DeletedBy = username;
            var deleteResult = await repository.DeleteAsync(entity, cancellationToken);
            if (deleteResult.IsFailure)
                return Result.Fail<DeleteTeamCommandResult>(deleteResult.Error, deleteResult.ErrorKey);
        }
        else
        {
            entity.Active    = false;
            entity.DeletedAt = DateTime.Now;
            entity.DeletedBy = username;
            var updateResult = await repository.UpdateAsync(entity, cancellationToken);
            if (updateResult.IsFailure)
                return Result.Fail<DeleteTeamCommandResult>(updateResult.Error, updateResult.ErrorKey);
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(new DeleteTeamCommandResult { Id = entity.Id });
    }
}

