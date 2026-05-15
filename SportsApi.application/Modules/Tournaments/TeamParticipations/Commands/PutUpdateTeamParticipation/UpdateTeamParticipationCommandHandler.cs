using SportsApi.application.Modules.Tournaments.TeamParticipations.Filters;
using SportsApi.domain.Abstractions.Auth;
using SportsApi.domain.Abstractions.Dtos;
using SportsApi.domain.Abstractions.Messaging.Commands;
using SportsApi.domain.Abstractions.Persistence;
using SportsApi.domain.Modules.Tournaments;

namespace SportsApi.application.Modules.Tournaments.TeamParticipations.Commands.PutUpdateTeamParticipation;

public class UpdateTeamParticipationCommandHandler(
    IRepository<TeamParticipation> repository,
    ICoreUnitOfWork unitOfWork,
    ICurrentUser currentUser) : ICommandHandler<UpdateTeamParticipationCommand, UpdateTeamParticipationCommandResult>
{
    public async Task<Result<UpdateTeamParticipationCommandResult>> HandleAsync(
        UpdateTeamParticipationCommand command,
        CancellationToken cancellationToken)
    {
        var entity = await repository.GetBySpecificationAsync(
            new TeamParticipationByIdFilter(command.Id), cancellationToken);

        if (entity is null)
            return Result.Fail<UpdateTeamParticipationCommandResult>("TeamParticipation not found", "TEAM_PARTICIPATION_NOT_FOUND");

        if (command.Name    is not null) entity.Name    = command.Name;
        if (command.LogoUrl is not null) entity.LogoUrl = command.LogoUrl;

        entity.UpdatedAt = DateTime.UtcNow;
        entity.UpdatedBy = currentUser.Username;

        var updateResult = await repository.UpdateAsync(entity, cancellationToken);
        if (updateResult.IsFailure)
            return Result.Fail<UpdateTeamParticipationCommandResult>(updateResult.Error, updateResult.ErrorKey);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(new UpdateTeamParticipationCommandResult { Id = entity.Id });
    }
}

