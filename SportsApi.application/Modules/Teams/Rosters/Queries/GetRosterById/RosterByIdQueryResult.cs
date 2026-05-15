using SportsApi.domain.Abstractions.Messaging.Queries;
using SportsApi.domain.Modules.Teams;

namespace SportsApi.application.Modules.Teams.Rosters.Queries.GetRosterById;

public class RosterByIdQueryResult : IQueryResult
{
    public required Roster Data { get; set; }
}

