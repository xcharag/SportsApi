using SportsApi.domain.Abstractions.Messaging.Queries;

namespace SportsApi.application.Modules.Teams.Rosters.Queries.GetAllRosters;

public class AllRostersQuery : IQuery<AllRostersQueryResult>
{
    public int   Page               { get; set; } = 1;
    public int   PerPage            { get; set; } = 20;
    public Guid? TeamParticipationId { get; set; }
    public Guid? PlayerId           { get; set; }
}

