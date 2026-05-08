using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SportsApi.domain.Modules.Teams;

namespace SportsApi.infrastructure.Persistence.Core.EntityFramework.Configurations;

public class TeamConfigurations : BaseEntityConfiguration<Team>
{
    protected override void ConfigureEntity(EntityTypeBuilder<Team> builder)
    {
        builder.ToTable("Teams");
        
        builder.Property(t => t.DefaultName)
            .IsRequired()
            .HasMaxLength(100);
        
        builder.HasMany(t => t.Rosters)
            .WithOne(r => r.Team)
            .HasForeignKey(r => r.TeamId)
            .OnDelete(DeleteBehavior.Restrict);
        
        builder.HasMany(t => t.TeamParticipations)
            .WithOne(tp => tp.Team)
            .HasForeignKey(tp => tp.TeamId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}