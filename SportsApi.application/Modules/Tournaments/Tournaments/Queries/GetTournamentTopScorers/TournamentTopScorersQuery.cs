using SportsApi.domain.Abstractions.Messaging.Queries;

namespace SportsApi.application.Modules.Tournaments.Tournaments.Queries.GetTournamentTopScorers;

public class TournamentTopScorersQuery : IQuery<TournamentTopScorersQueryResult>
{
    public Guid TournamentId { get; set; }
    /// <summary>Maximum number of top scorers to return (default 10).</summary>
    public int Limit { get; set; } = 10;
}
