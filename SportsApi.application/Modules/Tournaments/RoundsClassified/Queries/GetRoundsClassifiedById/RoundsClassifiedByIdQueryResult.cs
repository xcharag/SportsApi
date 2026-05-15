using SportsApi.domain.Abstractions.Messaging.Queries;

namespace SportsApi.application.Modules.Tournaments.RoundsClassified.Queries.GetRoundsClassifiedById;

public class RoundsClassifiedByIdQueryResult : IQueryResult
{
    public required domain.Modules.Tournaments.RoundsClassified Data { get; set; }
}

