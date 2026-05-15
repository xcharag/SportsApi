using SportsApi.domain.Abstractions.Messaging.Queries;

namespace SportsApi.application.Modules.Teams.Teams.Queries.GetTeamProfile;

public class TeamProfileQuery : IQuery<TeamProfileQueryResult>
{
    public Guid TeamId { get; set; }
}
