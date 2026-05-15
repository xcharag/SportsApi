using SportsApi.domain.Abstractions.Messaging.Queries;

namespace SportsApi.application.Modules.Teams.Players.Queries.GetPlayerStats;

public class PlayerStatsQuery : IQuery<PlayerStatsQueryResult>
{
    public Guid PlayerId { get; set; }
}
