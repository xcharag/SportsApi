using SportsApi.domain.Abstractions.Messaging.Queries;

namespace SportsApi.application.Modules.Teams.Teams.Queries.GetAllTeams;

public class AllTeamsQuery : IQuery<AllTeamsQueryResult>
{
    public int Page { get; set; } = 1;
    public int PerPage { get; set; } = 20;
    public string? Name { get; set; }
}

