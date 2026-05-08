using SportsApi.domain.Abstractions.Entities;
using SportsApi.domain.Modules.Matches;

namespace SportsApi.domain.Modules.Teams;

public class Roster : BaseEntity
{
    public int? ShirtNumber { get; set; }
    public string? ShirtName { get; set; }
    
    public Guid PlayerId { get; set; }
    public Player Player { get; set; } = null!;
    
    public Guid TeamId { get; set; }
    public Team Team { get; set; } = null!;
    
    public ICollection<Event>? Events { get; set; }
}