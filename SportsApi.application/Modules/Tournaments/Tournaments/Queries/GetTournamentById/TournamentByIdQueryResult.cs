using SportsApi.domain.Abstractions.Messaging.Queries;
using SportsApi.domain.Modules.Tournaments;

namespace SportsApi.application.Modules.Tournaments.Tournaments.Queries.GetTournamentById;

public class TournamentByIdQueryResult : IQueryResult
{
    public required Tournament Data { get; set; }
}