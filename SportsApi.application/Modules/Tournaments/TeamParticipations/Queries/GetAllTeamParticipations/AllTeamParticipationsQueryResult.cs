using SportsApi.domain.Abstractions.Dtos;
using SportsApi.domain.Abstractions.Messaging.Queries;
using SportsApi.domain.Modules.Tournaments;

namespace SportsApi.application.Modules.Tournaments.TeamParticipations.Queries.GetAllTeamParticipations;

public class AllTeamParticipationsQueryResult : IQueryResult
{
    public required PaginationResult<TeamParticipation> Data { get; set; }
    public int TotalCount   { get; set; }
    public int ActiveCount  { get; set; }
    public int InactiveCount { get; set; }
}

