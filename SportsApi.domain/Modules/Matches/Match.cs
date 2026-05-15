using SportsApi.domain.Abstractions.Entities;
using SportsApi.domain.Enums;
using SportsApi.domain.Enums.Status;
using SportsApi.domain.Modules.Tournaments;

namespace SportsApi.domain.Modules.Matches;

public class Match : BaseEntity
{
    public int MatchDay { get; set; }
    public int ScoreHomeTeam { get; set; }
    public int ScoreAwayTeam { get; set; }
    public DateTime MatchDate { get; set; }
    public string? Field { get; set; }
    public string? Location { get; set; }
    public MatchStatus Status { get; set; } = MatchStatus.Pending;
    public MatchRound Round { get; set; }
    public Guid? NewMatchId { get; set; }
    
    public Guid HomeTeamId { get; set; }
    public TeamParticipation HomeTeam { get; set; } = null!;
        
    public Guid AwayTeamId { get; set; }
    public TeamParticipation AwayTeam { get; set; } = null!;
    
    public ICollection<Event>? Events { get; set; }
}