using SportsApi.application.Modules.Teams.Players.Filters;
using SportsApi.domain.Abstractions.Dtos;
using SportsApi.domain.Abstractions.Messaging.Queries;
using SportsApi.domain.Abstractions.Persistence;
using SportsApi.domain.Modules.Teams;

namespace SportsApi.application.Modules.Teams.Players.Queries.GetPlayerById;

public class PlayerByIdQueryHandler(IRepository<Player> repository)
    : IQueryHandler<PlayerByIdQuery, PlayerByIdQueryResult>
{
    public async Task<Result<PlayerByIdQueryResult>> HandleAsync(
        PlayerByIdQuery query,
        CancellationToken cancellationToken)
    {
        var entity = await repository.GetBySpecificationAsync(new PlayerByIdFilter(query.Id), cancellationToken);

        if (entity is null)
            return Result.Fail<PlayerByIdQueryResult>("Player not found", "PLAYER_NOT_FOUND");

        return Result.Success(new PlayerByIdQueryResult { Data = entity });
    }
}

