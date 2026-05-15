using SportsApi.application.Modules.Teams.Rosters.Filters;
using SportsApi.domain.Abstractions.Dtos;
using SportsApi.domain.Abstractions.Messaging.Queries;
using SportsApi.domain.Abstractions.Persistence;
using SportsApi.domain.Modules.Teams;

namespace SportsApi.application.Modules.Teams.Rosters.Queries.GetRosterById;

public class RosterByIdQueryHandler(IRepository<Roster> repository)
    : IQueryHandler<RosterByIdQuery, RosterByIdQueryResult>
{
    public async Task<Result<RosterByIdQueryResult>> HandleAsync(
        RosterByIdQuery query,
        CancellationToken cancellationToken)
    {
        var entity = await repository.GetBySpecificationAsync(new RosterByIdFilter(query.Id), cancellationToken);

        if (entity is null)
            return Result.Fail<RosterByIdQueryResult>("Roster not found", "ROSTER_NOT_FOUND");

        return Result.Success(new RosterByIdQueryResult { Data = entity });
    }
}

