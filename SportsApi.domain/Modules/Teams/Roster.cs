using SportsApi.domain.Abstractions.Entities;
using SportsApi.domain.Modules.Matches;
using SportsApi.domain.Modules.Tournaments;

namespace SportsApi.domain.Modules.Teams;

public class Roster : BaseEntity
{
    public int? ShirtNumber { get; set; }
    public string? ShirtName { get; set; }
    
    public Guid PlayerId { get; set; }
    public Player Player { get; set; } = null!;
    
    public Guid TeamParticipationId { get; set; }
    public TeamParticipation Team { get; set; } = null!;
    
    public ICollection<Event>? Events { get; set; }
}