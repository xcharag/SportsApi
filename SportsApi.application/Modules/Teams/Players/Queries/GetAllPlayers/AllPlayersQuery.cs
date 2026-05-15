using SportsApi.domain.Abstractions.Messaging.Queries;

namespace SportsApi.application.Modules.Teams.Players.Queries.GetAllPlayers;

public class AllPlayersQuery : IQuery<AllPlayersQueryResult>
{
    public int    Page        { get; set; } = 1;
    public int    PerPage     { get; set; } = 20;
    public string? Name       { get; set; }
    public bool?  IsForeigner { get; set; }
}

