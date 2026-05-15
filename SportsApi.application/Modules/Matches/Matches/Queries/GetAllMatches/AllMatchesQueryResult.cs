using SportsApi.domain.Abstractions.Dtos;
using SportsApi.domain.Abstractions.Messaging.Queries;
using SportsApi.domain.Modules.Matches;

namespace SportsApi.application.Modules.Matches.Matches.Queries.GetAllMatches;

public class AllMatchesQueryResult : IQueryResult
{
    public required PaginationResult<Match> Data { get; set; }
    public int TotalCount   { get; set; }
    public int ActiveCount  { get; set; }
    public int InactiveCount { get; set; }
}

