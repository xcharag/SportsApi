using SportsApi.domain.Abstractions.Messaging.Queries;

namespace SportsApi.application.Modules.Teams.Teams.Queries.GetTeamById;

public class TeamByIdQuery : IQuery<TeamByIdQueryResult>
{
    public Guid Id { get; set; }
}

