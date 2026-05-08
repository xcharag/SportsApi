using SportsApi.domain.Abstractions.Entities;
using SportsApi.domain.Enums;
using SportsApi.domain.Enums.Types;
using SportsApi.domain.Modules.Teams;

namespace SportsApi.domain.Modules.Matches;

public class Event : BaseEntity
{
    public int Minute { get; set; }
    public FavorableTo FavorableTo { get; set; }
    public EventType EventType { get; set; }
    
    public Guid RosterId { get; set; }
    public Roster Roster { get; set; } = null!;
    
    public Guid MatchId { get; set; }
    public Match Match { get; set; } = null!;
}