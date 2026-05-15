using SportsApi.domain.Abstractions.Messaging.Queries;

namespace SportsApi.application.Modules.Tournaments.Tournaments.Queries.GetAllTournaments;

public class AllTournamentsQuery : IQuery<AllTournamentsQueryResult>
{
    public int Page { get; set; } = 1;
    public int PerPage { get; set; } = 20;
    public string? Name { get; set; }
    public DateTime? InitDate { get; set; }
    public DateTime? EndDate { get; set; }
}