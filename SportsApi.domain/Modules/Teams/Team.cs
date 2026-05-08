using SportsApi.domain.Abstractions.Entities;
using SportsApi.domain.Modules.Matches;
using SportsApi.domain.Modules.Tournaments;

namespace SportsApi.domain.Modules.Teams;

public class Team : BaseEntity
{
    public string DefaultName { get; set; } = null!;
    public string? DefaultLogoUrl { get; set; }
    
    public ICollection<TeamParticipation>? TeamParticipations { get; set; } 
    public ICollection<Roster>? Rosters { get; set; }
}