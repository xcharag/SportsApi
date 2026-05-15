using SportsApi.domain.Abstractions.Auth;
using SportsApi.domain.Abstractions.Dtos;
using SportsApi.domain.Abstractions.Messaging.Commands;
using SportsApi.domain.Abstractions.Persistence;
using SportsApi.domain.Modules.Matches;

namespace SportsApi.application.Modules.Matches.Matches.Commands.PostCreateMatch;

public class CreateMatchCommandHandler(
    IRepository<Match> repository,
    ICoreUnitOfWork unitOfWork,
    ICurrentUser currentUser) : ICommandHandler<CreateMatchCommand, CreateMatchCommandResult>
{
    public async Task<Result<CreateMatchCommandResult>> HandleAsync(
        CreateMatchCommand command,
        CancellationToken cancellationToken)
    {
        var entity = new Match
        {
            MatchDay     = command.MatchDay,
            MatchDate    = command.MatchDate,
            Field        = command.Field,
            Location     = command.Location,
            Status       = command.Status,
            HomeTeamId   = command.HomeTeamId,
            AwayTeamId   = command.AwayTeamId,
            NewMatchId   = command.NewMatchId,
            ScoreHomeTeam = 0,
            ScoreAwayTeam = 0,
            CreatedBy    = currentUser.Username
        };

        var saveResult = await repository.SaveAsync(entity, cancellationToken);
        if (saveResult.IsFailure)
            return Result.Fail<CreateMatchCommandResult>(saveResult.Error);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(new CreateMatchCommandResult { Id = entity.Id });
    }
}

