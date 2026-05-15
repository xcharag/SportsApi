using SportsApi.application.Modules.Tournaments.RoundsClassified.Filters;
using SportsApi.domain.Abstractions.Dtos;
using SportsApi.domain.Abstractions.Messaging.Queries;
using SportsApi.domain.Abstractions.Persistence;

namespace SportsApi.application.Modules.Tournaments.RoundsClassified.Queries.GetRoundsClassifiedById;

public class RoundsClassifiedByIdQueryHandler(
    IRepository<domain.Modules.Tournaments.RoundsClassified> repository)
    : IQueryHandler<RoundsClassifiedByIdQuery, RoundsClassifiedByIdQueryResult>
{
    public async Task<Result<RoundsClassifiedByIdQueryResult>> HandleAsync(
        RoundsClassifiedByIdQuery query,
        CancellationToken cancellationToken)
    {
        var entity = await repository.GetBySpecificationAsync(
            new RoundsClassifiedByIdFilter(query.Id), cancellationToken);

        if (entity is null)
            return Result.Fail<RoundsClassifiedByIdQueryResult>(
                "RoundsClassified record not found", "ROUNDS_CLASSIFIED_NOT_FOUND");

        return Result.Success(new RoundsClassifiedByIdQueryResult { Data = entity });
    }
}

