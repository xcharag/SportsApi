using SportsApi.domain.Abstractions.Messaging.Queries;

namespace SportsApi.application.Modules.Tournaments.RoundsClassified.Queries.GetRoundsClassifiedById;

public class RoundsClassifiedByIdQuery : IQuery<RoundsClassifiedByIdQueryResult>
{
    public Guid Id { get; set; }
}

