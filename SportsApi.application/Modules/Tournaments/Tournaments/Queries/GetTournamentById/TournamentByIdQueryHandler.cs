using SportsApi.application.Modules.Tournaments.Tournaments.Filters;
using SportsApi.domain.Abstractions.Dtos;
using SportsApi.domain.Abstractions.Messaging.Queries;
using SportsApi.domain.Abstractions.Persistence;
using SportsApi.domain.Modules.Tournaments;

namespace SportsApi.application.Modules.Tournaments.Tournaments.Queries.GetTournamentById;

public class TournamentByIdQueryHandler(IRepository<Tournament> repository)
    : IQueryHandler<TournamentByIdQuery, TournamentByIdQueryResult>
{
    public async Task<Result<TournamentByIdQueryResult>> HandleAsync(
        TournamentByIdQuery query, 
        CancellationToken cancellationToken)
    {
        var filter = new TournamentByIdFilter(query.Id);
        var entity = await repository.GetBySpecificationAsync(filter, cancellationToken);
        
        if (entity is null)
            return Result.Fail<TournamentByIdQueryResult>("Tournament not found", "TOURNAMENT_NOT_FOUND");

        return Result.Success(new TournamentByIdQueryResult { Data = entity });
    }
}