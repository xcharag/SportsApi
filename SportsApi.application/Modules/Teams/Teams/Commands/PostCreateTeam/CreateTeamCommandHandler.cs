using SportsApi.domain.Abstractions.Auth;
using SportsApi.domain.Abstractions.Dtos;
using SportsApi.domain.Abstractions.Messaging.Commands;
using SportsApi.domain.Abstractions.Persistence;
using SportsApi.domain.Modules.Teams;

namespace SportsApi.application.Modules.Teams.Teams.Commands.PostCreateTeam;

public class CreateTeamCommandHandler(
    IRepository<Team> repository,
    ICoreUnitOfWork unitOfWork,
    ICurrentUser currentUser) : ICommandHandler<CreateTeamCommand, CreateTeamCommandResult>
{
    public async Task<Result<CreateTeamCommandResult>> HandleAsync(
        CreateTeamCommand command,
        CancellationToken cancellationToken)
    {
        var entity = new Team
        {
            DefaultName   = command.DefaultName,
            DefaultLogoUrl = command.DefaultLogoUrl,
            CreatedBy     = currentUser.Username
        };

        var saveResult = await repository.SaveAsync(entity, cancellationToken);
        if (saveResult.IsFailure)
            return Result.Fail<CreateTeamCommandResult>(saveResult.Error);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(new CreateTeamCommandResult { Id = entity.Id, DefaultName = entity.DefaultName });
    }
}

