using SportsApi.domain.Abstractions.Dtos;
using SportsApi.domain.Abstractions.Messaging.Queries;
using SportsApi.domain.Modules.Teams;

namespace SportsApi.application.Modules.Teams.Rosters.Queries.GetAllRosters;

public class AllRostersQueryResult : IQueryResult
{
    public required PaginationResult<Roster> Data { get; set; }
    public int TotalCount   { get; set; }
    public int ActiveCount  { get; set; }
    public int InactiveCount { get; set; }
}

