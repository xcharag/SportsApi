using SportsApi.application.Modules.Teams.Teams.Filters;
using SportsApi.domain.Abstractions.Auth;
using SportsApi.domain.Abstractions.Dtos;
using SportsApi.domain.Abstractions.Messaging.Commands;
using SportsApi.domain.Abstractions.Persistence;
using SportsApi.domain.Modules.Teams;

namespace SportsApi.application.Modules.Teams.Teams.Commands.PutUpdateTeam;

public class UpdateTeamCommandHandler(
    IRepository<Team> repository,
    ICoreUnitOfWork unitOfWork,
    ICurrentUser currentUser) : ICommandHandler<UpdateTeamCommand, UpdateTeamCommandResult>
{
    public async Task<Result<UpdateTeamCommandResult>> HandleAsync(
        UpdateTeamCommand command,
        CancellationToken cancellationToken)
    {
        var entity = await repository.GetBySpecificationAsync(new TeamByIdFilter(command.Id), cancellationToken);

        if (entity is null)
            return Result.Fail<UpdateTeamCommandResult>("Team not found", "TEAM_NOT_FOUND");

        if (command.DefaultName    is not null) entity.DefaultName    = command.DefaultName;
        if (command.DefaultLogoUrl is not null) entity.DefaultLogoUrl = command.DefaultLogoUrl;

        entity.UpdatedAt = DateTime.UtcNow;
        entity.UpdatedBy = currentUser.Username;

        var updateResult = await repository.UpdateAsync(entity, cancellationToken);
        if (updateResult.IsFailure)
            return Result.Fail<UpdateTeamCommandResult>(updateResult.Error, updateResult.ErrorKey);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(new UpdateTeamCommandResult { Id = entity.Id });
    }
}

