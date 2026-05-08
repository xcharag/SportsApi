using SportsApi.domain.Abstractions.Entities;
using SportsApi.domain.Modules.Matches;
using SportsApi.domain.Modules.Tournaments;

namespace SportsApi.domain.Modules.Teams;

public class Team : BaseEntity
{
    public string Name { get; set; } = null!;
    public string? LogoUrl { get; set; }
    
    public Guid TournamentId { get; set; }
    public Tournament Tournament { get; set; } = null!;
    
    public ICollection<Roster> Rosters { get; set; } = new List<Roster>();
    public ICollection<Match> Matches { get; set; } = new List<Match>();
}