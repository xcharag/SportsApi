using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SportsApi.domain.Modules.Teams;

namespace SportsApi.infrastructure.Persistence.Core.EntityFramework.Configurations;

public class RosterConfiguration : BaseEntityConfiguration<Roster>
{
    protected override void ConfigureEntity(EntityTypeBuilder<Roster> builder)
    {
        builder.ToTable("Rosters");
        
        builder.Property(r => r.ShirtName)
            .HasMaxLength(20);
        
        builder.HasOne(r => r.Team)
            .WithMany(t => t.Rosters)
            .HasForeignKey(r => r.TeamId)
            .OnDelete(DeleteBehavior.Restrict);
        
        builder.HasOne(r => r.Player)
            .WithMany(p => p.Rosters)
            .HasForeignKey(r => r.PlayerId)
            .OnDelete(DeleteBehavior.Restrict);
        
        builder.HasMany(r => r.Events)
            .WithOne(e => e.Roster)
            .HasForeignKey(e => e.RosterId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}