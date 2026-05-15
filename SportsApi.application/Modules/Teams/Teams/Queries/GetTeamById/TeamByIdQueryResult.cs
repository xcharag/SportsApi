using SportsApi.domain.Abstractions.Messaging.Queries;
using SportsApi.domain.Modules.Teams;

namespace SportsApi.application.Modules.Teams.Teams.Queries.GetTeamById;

public class TeamByIdQueryResult : IQueryResult
{
    public required Team Data { get; set; }
}

