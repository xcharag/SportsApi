using SportsApi.application.Modules.Tournaments.TeamParticipations.Filters;
using SportsApi.domain.Abstractions.Dtos;
using SportsApi.domain.Abstractions.Messaging.Queries;
using SportsApi.domain.Abstractions.Persistence;
using SportsApi.domain.Modules.Tournaments;

namespace SportsApi.application.Modules.Tournaments.TeamParticipations.Queries.GetTeamParticipationById;

public class TeamParticipationByIdQueryHandler(IRepository<TeamParticipation> repository)
    : IQueryHandler<TeamParticipationByIdQuery, TeamParticipationByIdQueryResult>
{
    public async Task<Result<TeamParticipationByIdQueryResult>> HandleAsync(
        TeamParticipationByIdQuery query,
        CancellationToken cancellationToken)
    {
        var entity = await repository.GetBySpecificationAsync(
            new TeamParticipationByIdFilter(query.Id), cancellationToken);

        if (entity is null)
            return Result.Fail<TeamParticipationByIdQueryResult>("TeamParticipation not found", "TEAM_PARTICIPATION_NOT_FOUND");

        return Result.Success(new TeamParticipationByIdQueryResult { Data = entity });
    }
}

