namespace SportsApi.domain.Abstractions.Entities;

public class BaseEntity : Entity
{
    public Guid Id { get; set; }
    public bool Active { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.Now;
    public DateTime? UpdatedAt { get; set; }
    public DateTime? DeletedAt { get; set; }
    public string CreatedBy { get; set; } = null!;
    public string? UpdatedBy { get; set; }
    public string? DeletedBy { get; set; }
    
    public Guid TournamentIdOwner { get; set; }
}