using SportsApi.application.Modules.Matches.Matches.Filters;
using SportsApi.domain.Abstractions.Auth;
using SportsApi.domain.Abstractions.Dtos;
using SportsApi.domain.Abstractions.Messaging.Commands;
using SportsApi.domain.Abstractions.Persistence;
using SportsApi.domain.Modules.Matches;

namespace SportsApi.application.Modules.Matches.Matches.Commands.PutUpdateMatch;

public class UpdateMatchCommandHandler(
    IRepository<Match> repository,
    ICoreUnitOfWork unitOfWork,
    ICurrentUser currentUser) : ICommandHandler<UpdateMatchCommand, UpdateMatchCommandResult>
{
    public async Task<Result<UpdateMatchCommandResult>> HandleAsync(
        UpdateMatchCommand command,
        CancellationToken cancellationToken)
    {
        var entity = await repository.GetBySpecificationAsync(new MatchByIdFilter(command.Id), cancellationToken);

        if (entity is null)
            return Result.Fail<UpdateMatchCommandResult>("Match not found", "MATCH_NOT_FOUND");

        if (command.MatchDay      is not null) entity.MatchDay      = command.MatchDay.Value;
        if (command.MatchDate     is not null) entity.MatchDate     = command.MatchDate.Value;
        if (command.Field         is not null) entity.Field         = command.Field;
        if (command.Location      is not null) entity.Location      = command.Location;
        if (command.Status        is not null) entity.Status        = command.Status.Value;
        if (command.ScoreHomeTeam is not null) entity.ScoreHomeTeam = command.ScoreHomeTeam.Value;
        if (command.ScoreAwayTeam is not null) entity.ScoreAwayTeam = command.ScoreAwayTeam.Value;
        if (command.NewMatchId    is not null) entity.NewMatchId    = command.NewMatchId;

        entity.UpdatedAt = DateTime.Now;
        entity.UpdatedBy = currentUser.Username;

        var updateResult = await repository.UpdateAsync(entity, cancellationToken);
        if (updateResult.IsFailure)
            return Result.Fail<UpdateMatchCommandResult>(updateResult.Error, updateResult.ErrorKey);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(new UpdateMatchCommandResult { Id = entity.Id });
    }
}

