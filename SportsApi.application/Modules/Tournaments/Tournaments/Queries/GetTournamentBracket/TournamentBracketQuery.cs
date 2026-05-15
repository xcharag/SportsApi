using SportsApi.domain.Abstractions.Messaging.Queries;

namespace SportsApi.application.Modules.Tournaments.Tournaments.Queries.GetTournamentBracket;

public class TournamentBracketQuery : IQuery<TournamentBracketQueryResult>
{
    public Guid TournamentId { get; set; }
}
