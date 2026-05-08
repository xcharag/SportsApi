using SportsApi.domain.Abstractions.Entities;
using SportsApi.domain.Modules.Teams;

namespace SportsApi.domain.Modules.Tournaments;

public class Tournament : BaseEntity
{
    public string Name { get; set; } = null!;
    public string Description { get; set; } = null!;
    
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    
    public string? LogoUrl { get; set; }
    public string? BannerUrl { get; set; }
    
    public ICollection<Team> Teams { get; set; } = new List<Team>();
}