using SportsApi.domain.Abstractions.Messaging.Queries;

namespace SportsApi.application.Modules.Teams.Rosters.Queries.GetRosterById;

public class RosterByIdQuery : IQuery<RosterByIdQueryResult>
{
    public Guid Id { get; set; }
}

