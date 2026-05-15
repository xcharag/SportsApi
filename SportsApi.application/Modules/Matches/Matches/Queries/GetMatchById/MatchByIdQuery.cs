using SportsApi.domain.Abstractions.Messaging.Queries;

namespace SportsApi.application.Modules.Matches.Matches.Queries.GetMatchById;

public class MatchByIdQuery : IQuery<MatchByIdQueryResult>
{
    public Guid Id { get; set; }
}

