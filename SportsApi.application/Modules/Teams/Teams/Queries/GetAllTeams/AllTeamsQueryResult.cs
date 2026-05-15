using SportsApi.domain.Abstractions.Dtos;
using SportsApi.domain.Abstractions.Messaging.Queries;
using SportsApi.domain.Modules.Teams;

namespace SportsApi.application.Modules.Teams.Teams.Queries.GetAllTeams;

public class AllTeamsQueryResult : IQueryResult
{
    public required PaginationResult<Team> Data { get; set; }
    public int TotalCount  { get; set; }
    public int ActiveCount { get; set; }
    public int InactiveCount { get; set; }
}

