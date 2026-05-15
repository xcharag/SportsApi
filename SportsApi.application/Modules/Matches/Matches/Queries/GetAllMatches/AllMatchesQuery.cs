using SportsApi.domain.Abstractions.Messaging.Queries;
using SportsApi.domain.Enums.Status;

namespace SportsApi.application.Modules.Matches.Matches.Queries.GetAllMatches;

public class AllMatchesQuery : IQuery<AllMatchesQueryResult>
{
    public int          Page       { get; set; } = 1;
    public int          PerPage    { get; set; } = 20;
    public Guid?        HomeTeamId { get; set; }
    public Guid?        AwayTeamId { get; set; }
    public MatchStatus? Status     { get; set; }
    public DateTime?    FromDate   { get; set; }
    public DateTime?    ToDate     { get; set; }
}

