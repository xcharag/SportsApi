using SportsApi.domain.Abstractions.Entities;

namespace SportsApi.domain.Modules.Teams;

public class Player : BaseEntity
{
    public string FullName { get; set; } = null!;
    public string? Ci { get; set; } 
    public string? PhoneNumber { get; set; }
    public bool IsForeigner { get; set; }
    
    public ICollection<Roster>? Rosters { get; set; }
}