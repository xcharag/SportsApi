using SportsApi.domain.Abstractions.Entities;
using SportsApi.domain.Enums;

namespace SportsApi.domain.Modules.Tournaments;

public class RoundsClassified : BaseEntity
{
    public MatchRound Round { get; set; }
    public int? GroupPosition { get; set; }
    public string RoundKey { get; set; } = null!;
    public string? NextRoundKey { get; set; }
    
    public Guid TeamParticipationId { get; set; }
    public TeamParticipation? TeamParticipation { get; set; }
}