using SportsApi.domain.Abstractions.Messaging.Queries;

namespace SportsApi.application.Modules.Teams.Players.Queries.GetPlayerProfile;

public class PlayerProfileQuery : IQuery<PlayerProfileQueryResult>
{
    public Guid PlayerId { get; set; }
}
