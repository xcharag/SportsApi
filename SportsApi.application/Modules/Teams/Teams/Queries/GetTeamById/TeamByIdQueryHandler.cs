using SportsApi.application.Modules.Teams.Teams.Filters;
using SportsApi.domain.Abstractions.Dtos;
using SportsApi.domain.Abstractions.Messaging.Queries;
using SportsApi.domain.Abstractions.Persistence;
using SportsApi.domain.Modules.Teams;

namespace SportsApi.application.Modules.Teams.Teams.Queries.GetTeamById;

public class TeamByIdQueryHandler(IRepository<Team> repository)
    : IQueryHandler<TeamByIdQuery, TeamByIdQueryResult>
{
    public async Task<Result<TeamByIdQueryResult>> HandleAsync(
        TeamByIdQuery query,
        CancellationToken cancellationToken)
    {
        var entity = await repository.GetBySpecificationAsync(new TeamByIdFilter(query.Id), cancellationToken);

        if (entity is null)
            return Result.Fail<TeamByIdQueryResult>("Team not found", "TEAM_NOT_FOUND");

        return Result.Success(new TeamByIdQueryResult { Data = entity });
    }
}

