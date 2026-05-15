using SportsApi.domain.Abstractions.Messaging.Queries;
using SportsApi.domain.Modules.Tournaments;

namespace SportsApi.application.Modules.Tournaments.TeamParticipations.Queries.GetTeamParticipationById;

public class TeamParticipationByIdQueryResult : IQueryResult
{
    public required TeamParticipation Data { get; set; }
}

