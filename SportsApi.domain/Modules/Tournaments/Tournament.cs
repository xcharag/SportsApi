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

    /// <summary>How many teams per group advance to the knockout stage. Default 2.</summary>
    public int TeamsPerGroupThatClassify { get; set; } = 2;

    public ICollection<TeamParticipation>? TeamsParticipations { get; set; } = new List<TeamParticipation>();
}