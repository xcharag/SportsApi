using SportsApi.application.Modules.Tournaments.Tournaments.Filters;
using SportsApi.domain.Abstractions.Auth;
using SportsApi.domain.Abstractions.Dtos;
using SportsApi.domain.Abstractions.Messaging.Commands;
using SportsApi.domain.Abstractions.Persistence;
using SportsApi.domain.Modules.Tournaments;

namespace SportsApi.application.Modules.Tournaments.Tournaments.Commands.DeleteTournament;

public class DeleteTournamentCommandHandler(
    IRepository<Tournament> repository,
    ICoreUnitOfWork unitOfWork,
    ICurrentUser currentUser)
    : ICommandHandler<DeleteTournamentCommand, DeleteTournamentCommandResult>
{
    public async Task<Result<DeleteTournamentCommandResult>> HandleAsync(
        DeleteTournamentCommand command,
        CancellationToken cancellationToken)
    {
        var filter = new TournamentByIdFilter(command.Id);
        var entity = await repository.GetBySpecificationAsync(filter, cancellationToken);
        
        if (entity is null)
            return Result.Fail<DeleteTournamentCommandResult>("Torneo no encontrado", "TOURNAMENT_NOT_FOUND");
        
        var username = currentUser.Username;
        if (command.HardDelete)
        {
            entity.DeletedAt = DateTime.UtcNow;
            entity.DeletedBy = username;
            var deleteResult = await repository.DeleteAsync(entity, cancellationToken);
            if (deleteResult.IsFailure)
                return Result.Fail<DeleteTournamentCommandResult>(deleteResult.Error, deleteResult.ErrorKey);
        }
        else
        {
            entity.Active = false;
            entity.DeletedAt = DateTime.UtcNow;
            entity.DeletedBy = username;
            var updateResult = await repository.UpdateAsync(entity, cancellationToken);
            if (updateResult.IsFailure)
                return Result.Fail<DeleteTournamentCommandResult>(updateResult.Error, updateResult.ErrorKey);
        }
        
        await unitOfWork.SaveChangesAsync(cancellationToken);
        
        return Result.Success(new DeleteTournamentCommandResult { Id = entity.Id });
    }
}