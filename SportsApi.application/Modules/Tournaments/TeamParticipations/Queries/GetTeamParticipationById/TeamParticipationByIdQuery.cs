using SportsApi.domain.Abstractions.Messaging.Queries;

namespace SportsApi.application.Modules.Tournaments.TeamParticipations.Queries.GetTeamParticipationById;

public class TeamParticipationByIdQuery : IQuery<TeamParticipationByIdQueryResult>
{
    public Guid Id { get; set; }
}

