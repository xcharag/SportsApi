using SportsApi.domain.Abstractions.Auth;
using SportsApi.domain.Abstractions.Dtos;
using SportsApi.domain.Abstractions.Messaging.Commands;
using SportsApi.domain.Abstractions.Persistence;
using SportsApi.domain.Modules.Tournaments;

namespace SportsApi.application.Modules.Tournaments.TeamParticipations.Commands.PostCreateTeamParticipation;

public class CreateTeamParticipationCommandHandler(
    IRepository<TeamParticipation> repository,
    ICoreUnitOfWork unitOfWork,
    ICurrentUser currentUser) : ICommandHandler<CreateTeamParticipationCommand, CreateTeamParticipationCommandResult>
{
    public async Task<Result<CreateTeamParticipationCommandResult>> HandleAsync(
        CreateTeamParticipationCommand command,
        CancellationToken cancellationToken)
    {
        var entity = new TeamParticipation
        {
            Name         = command.Name,
            LogoUrl      = command.LogoUrl,
            TeamId       = command.TeamId,
            TournamentId = command.TournamentId,
            CreatedBy    = currentUser.Username
        };

        var saveResult = await repository.SaveAsync(entity, cancellationToken);
        if (saveResult.IsFailure)
            return Result.Fail<CreateTeamParticipationCommandResult>(saveResult.Error);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(new CreateTeamParticipationCommandResult { Id = entity.Id, Name = entity.Name });
    }
}

