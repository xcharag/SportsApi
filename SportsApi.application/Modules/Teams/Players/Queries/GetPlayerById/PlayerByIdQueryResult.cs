using SportsApi.domain.Abstractions.Messaging.Queries;
using SportsApi.domain.Modules.Teams;

namespace SportsApi.application.Modules.Teams.Players.Queries.GetPlayerById;

public class PlayerByIdQueryResult : IQueryResult
{
    public required Player Data { get; set; }
}

