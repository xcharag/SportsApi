using SportsApi.domain.Abstractions.Messaging.Queries;

namespace SportsApi.application.Modules.Tournaments.Tournaments.Queries.GetTournamentById;

public class TournamentByIdQuery : IQuery<TournamentByIdQueryResult>
{
    public Guid Id { get; set; }
}