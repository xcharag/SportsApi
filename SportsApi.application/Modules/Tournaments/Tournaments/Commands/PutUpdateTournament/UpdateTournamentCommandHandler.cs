using SportsApi.application.Modules.Tournaments.Tournaments.Filters;
using SportsApi.domain.Abstractions.Auth;
using SportsApi.domain.Abstractions.Dtos;
using SportsApi.domain.Abstractions.Messaging.Commands;
using SportsApi.domain.Abstractions.Persistence;
using SportsApi.domain.Modules.Tournaments;

namespace SportsApi.application.Modules.Tournaments.Tournaments.Commands.PutUpdateTournament;

public class UpdateTournamentCommandHandler(
    IRepository<Tournament> repository,
    ICoreUnitOfWork unitOfWork,
    ICurrentUser currentUser) 
    : ICommandHandler<UpdateTournamentCommand, UpdateTournamentCommandResult>
{
    public async Task<Result<UpdateTournamentCommandResult>> HandleAsync(
        UpdateTournamentCommand command,
        CancellationToken cancellationToken)
    {
        var filter = new TournamentByIdFilter(command.Id);
        var entity = await repository.GetBySpecificationAsync(filter, cancellationToken);
        
        if (entity is null)
            return Result.Fail<UpdateTournamentCommandResult>("Tournament not found", "TOURNAMENT_NOT_FOUND");

        var username = currentUser.Username;
        
        if (command.Name is not null)
            entity.Name = command.Name;
        if (command.Description is not null)
            entity.Description = command.Description;
        if (command.StartDate is not null)           
            entity.StartDate = command.StartDate.Value;
        if (command.EndDate is not null)
            entity.EndDate = command.EndDate.Value;
        if (command.LogoUrl is not null)
            entity.LogoUrl = command.LogoUrl;
        if (command.BannerUrl is not null)
            entity.BannerUrl = command.BannerUrl;
        
        entity.UpdatedAt = DateTime.Now;
        entity.UpdatedBy = username;
        
        var updateResult = await repository.UpdateAsync(entity, cancellationToken);
        if (updateResult.IsFailure)
            return Result.Fail<UpdateTournamentCommandResult>($"Failed to update tournament: {updateResult.Error}",
                updateResult.ErrorKey);
        
        await unitOfWork.SaveChangesAsync(cancellationToken);
        
        return Result.Success(new UpdateTournamentCommandResult { Id = entity.Id });
    }
}