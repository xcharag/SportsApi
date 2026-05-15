using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SportsApi.domain.Modules.Tournaments;

namespace SportsApi.infrastructure.Persistence.Core.EntityFramework.Configurations;

public class TeamParticipationConfiguration : BaseEntityConfiguration<TeamParticipation>
{
    protected override void ConfigureEntity(EntityTypeBuilder<TeamParticipation> builder)
    {
        builder.ToTable("TeamParticipations");
        
        builder.Property(tp => tp.Name)
            .IsRequired()
            .HasMaxLength(100);
        
        builder.HasOne(tp => tp.Team)
            .WithMany(t => t.TeamParticipations)
            .HasForeignKey(tp => tp.TeamId)
            .OnDelete(DeleteBehavior.Restrict);
        
        builder.HasOne(tp => tp.Tournament)
            .WithMany(t => t.TeamsParticipations)
            .HasForeignKey(tp => tp.TournamentId)
            .OnDelete(DeleteBehavior.Restrict);
        
        builder.HasMany(tp => tp.HomeMatches)
            .WithOne(m => m.HomeTeam)
            .HasForeignKey(m => m.HomeTeamId)
            .OnDelete(DeleteBehavior.Restrict);
        
        builder.HasMany(tp => tp.AwayMatches)
            .WithOne(m => m.AwayTeam)
            .HasForeignKey(m => m.AwayTeamId)
            .OnDelete(DeleteBehavior.Restrict);
        
        builder.HasMany(tp => tp.Rosters)
            .WithOne(r => r.Team)
            .HasForeignKey(r => r.TeamParticipationId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}