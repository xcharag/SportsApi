using SportsApi.domain.Abstractions.Dtos;
using SportsApi.domain.Abstractions.Messaging.Queries;
using SportsApi.domain.Modules.Teams;

namespace SportsApi.application.Modules.Teams.Players.Queries.GetAllPlayers;

public class AllPlayersQueryResult : IQueryResult
{
    public required PaginationResult<Player> Data { get; set; }
    public int TotalCount   { get; set; }
    public int ActiveCount  { get; set; }
    public int InactiveCount { get; set; }
}

