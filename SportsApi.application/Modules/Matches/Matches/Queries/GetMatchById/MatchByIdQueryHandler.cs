using SportsApi.application.Modules.Matches.Matches.Filters;
using SportsApi.domain.Abstractions.Dtos;
using SportsApi.domain.Abstractions.Messaging.Queries;
using SportsApi.domain.Abstractions.Persistence;
using SportsApi.domain.Modules.Matches;

namespace SportsApi.application.Modules.Matches.Matches.Queries.GetMatchById;

public class MatchByIdQueryHandler(IRepository<Match> repository)
    : IQueryHandler<MatchByIdQuery, MatchByIdQueryResult>
{
    public async Task<Result<MatchByIdQueryResult>> HandleAsync(
        MatchByIdQuery query,
        CancellationToken cancellationToken)
    {
        var entity = await repository.GetBySpecificationAsync(new MatchByIdFilter(query.Id), cancellationToken);

        if (entity is null)
            return Result.Fail<MatchByIdQueryResult>("Match not found", "MATCH_NOT_FOUND");

        return Result.Success(new MatchByIdQueryResult { Data = entity });
    }
}

