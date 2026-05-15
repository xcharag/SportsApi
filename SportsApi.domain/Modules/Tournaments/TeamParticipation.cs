using SportsApi.domain.Abstractions.Entities;
using SportsApi.domain.Modules.Matches;
using SportsApi.domain.Modules.Teams;

namespace SportsApi.domain.Modules.Tournaments;

public class TeamParticipation : BaseEntity
{
    public string Name { get; set; } = null!;
    public string? LogoUrl { get; set; }
    
    public Guid TeamId { get; set; }
    public Team Team { get; set; } = null!;
    
    public Guid TournamentId { get; set; }
    public Tournament Tournament { get; set; } = null!;
    
    public ICollection<Match>? HomeMatches { get; set; }
    public ICollection<Match>? AwayMatches { get; set; }
    public ICollection<Roster>? Rosters { get; set; }
}