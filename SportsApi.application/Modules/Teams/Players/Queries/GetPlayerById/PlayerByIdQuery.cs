using SportsApi.domain.Abstractions.Messaging.Queries;

namespace SportsApi.application.Modules.Teams.Players.Queries.GetPlayerById;

public class PlayerByIdQuery : IQuery<PlayerByIdQueryResult>
{
    public Guid Id { get; set; }
}

