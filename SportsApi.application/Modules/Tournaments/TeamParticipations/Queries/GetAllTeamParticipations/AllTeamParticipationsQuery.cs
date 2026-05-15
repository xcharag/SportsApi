using SportsApi.domain.Abstractions.Messaging.Queries;

namespace SportsApi.application.Modules.Tournaments.TeamParticipations.Queries.GetAllTeamParticipations;

public class AllTeamParticipationsQuery : IQuery<AllTeamParticipationsQueryResult>
{
    public int    Page         { get; set; } = 1;
    public int    PerPage      { get; set; } = 20;
    public Guid?  TournamentId { get; set; }
    public Guid?  TeamId       { get; set; }
    public string? Name        { get; set; }
}

