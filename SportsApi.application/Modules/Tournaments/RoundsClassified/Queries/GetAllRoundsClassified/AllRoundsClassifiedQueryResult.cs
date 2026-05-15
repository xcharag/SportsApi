using SportsApi.domain.Abstractions.Dtos;
using SportsApi.domain.Abstractions.Messaging.Queries;
using SportsApi.domain.Modules.Tournaments;

namespace SportsApi.application.Modules.Tournaments.RoundsClassified.Queries.GetAllRoundsClassified;

public class AllRoundsClassifiedQueryResult : IQueryResult
{
    public required PaginationResult<domain.Modules.Tournaments.RoundsClassified> Data { get; set; }
    public int ActiveCount   { get; set; }
    public int InactiveCount { get; set; }
    public int TotalCount    { get; set; }
}

