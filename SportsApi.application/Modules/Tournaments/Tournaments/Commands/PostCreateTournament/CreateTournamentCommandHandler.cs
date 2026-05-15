using SportsApi.domain.Abstractions.Auth;
using SportsApi.domain.Abstractions.Dtos;
using SportsApi.domain.Abstractions.Messaging.Commands;
using SportsApi.domain.Abstractions.Persistence;
using SportsApi.domain.Modules.Tournaments;

namespace SportsApi.application.Modules.Tournaments.Tournaments.Commands.PostCreateTournament;

public class CreateTournamentCommandHandler(
    IRepository<Tournament> repository,
    ICoreUnitOfWork unitOfWork,
    ICurrentUser currentUser) : ICommandHandler<CreateTournamentCommand, CreateTournamentCommandResult>
{
    public async Task<Result<CreateTournamentCommandResult>> HandleAsync(
        CreateTournamentCommand command,
        CancellationToken cancellationToken)
    {
        var username = currentUser.Username;
        var entity = new Tournament
        {
            Name = command.Name,
            Description = command.Description,
            StartDate = command.StartDate,
            EndDate = command.EndDate,
            LogoUrl = command.LogoUrl,
            BannerUrl = command.BannerUrl,
            TeamsPerGroupThatClassify = command.TeamsPerGroupThatClassify,
            CreatedBy = username
        };

        var saveResult = await repository.SaveAsync(entity, cancellationToken);
        if (saveResult.IsFailure)
            return Result.Fail<CreateTournamentCommandResult>(saveResult.Error);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(new CreateTournamentCommandResult { Id = entity.Id, Name = entity.Name });
    }
}