using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SportsApi.domain.Modules.Matches;

namespace SportsApi.infrastructure.Persistence.Core.EntityFramework.Configurations;

public class MatchConfiguration : BaseEntityConfiguration<Match>
{
    protected override void ConfigureEntity(EntityTypeBuilder<Match> builder)
    {
        builder.ToTable("Matches");
        
        builder.Property(m => m.Field)
            .HasMaxLength(30);
        
        builder.HasOne(m => m.HomeTeam)
            .WithMany(t => t.HomeMatches)
            .HasForeignKey(m => m.HomeTeamId)
            .OnDelete(DeleteBehavior.Restrict);
        
        builder.HasOne(m => m.AwayTeam)
            .WithMany(t => t.AwayMatches)
            .HasForeignKey(m => m.AwayTeamId)
            .OnDelete(DeleteBehavior.Restrict);
        
        builder.HasMany(m => m.Events)
            .WithOne(e => e.Match)
            .HasForeignKey(e => e.MatchId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}