using SportsApi.domain.Abstractions.Messaging.Queries;
using SportsApi.domain.Modules.Matches;

namespace SportsApi.application.Modules.Matches.Matches.Queries.GetMatchById;

public class MatchByIdQueryResult : IQueryResult
{
    public required Match Data { get; set; }
}

