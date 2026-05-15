using SportsApi.application.Modules.Matches.Matches.Filters;
using SportsApi.domain.Abstractions.Auth;
using SportsApi.domain.Abstractions.Dtos;
using SportsApi.domain.Abstractions.Messaging.Commands;
using SportsApi.domain.Abstractions.Persistence;
using SportsApi.domain.Modules.Matches;

namespace SportsApi.application.Modules.Matches.Matches.Commands.DeleteMatch;

public class DeleteMatchCommandHandler(
    IRepository<Match> repository,
    ICoreUnitOfWork unitOfWork,
    ICurrentUser currentUser) : ICommandHandler<DeleteMatchCommand, DeleteMatchCommandResult>
{
    public async Task<Result<DeleteMatchCommandResult>> HandleAsync(
        DeleteMatchCommand command,
        CancellationToken cancellationToken)
    {
        var entity = await repository.GetBySpecificationAsync(new MatchByIdFilter(command.Id), cancellationToken);

        if (entity is null)
            return Result.Fail<DeleteMatchCommandResult>("Match not found", "MATCH_NOT_FOUND");

        var username = currentUser.Username;

        if (command.HardDelete)
        {
            entity.DeletedAt = DateTime.UtcNow;
            entity.DeletedBy = username;
            var deleteResult = await repository.DeleteAsync(entity, cancellationToken);
            if (deleteResult.IsFailure)
                return Result.Fail<DeleteMatchCommandResult>(deleteResult.Error, deleteResult.ErrorKey);
        }
        else
        {
            entity.Active    = false;
            entity.DeletedAt = DateTime.UtcNow;
            entity.DeletedBy = username;
            var updateResult = await repository.UpdateAsync(entity, cancellationToken);
            if (updateResult.IsFailure)
                return Result.Fail<DeleteMatchCommandResult>(updateResult.Error, updateResult.ErrorKey);
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(new DeleteMatchCommandResult { Id = entity.Id });
    }
}

