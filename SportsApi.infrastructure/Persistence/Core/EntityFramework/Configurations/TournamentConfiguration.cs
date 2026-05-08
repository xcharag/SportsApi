using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SportsApi.domain.Modules.Tournaments;

namespace SportsApi.infrastructure.Persistence.Core.EntityFramework.Configurations;

public class TournamentConfiguration : BaseEntityConfiguration<Tournament>
{
    protected override void ConfigureEntity(EntityTypeBuilder<Tournament> builder)
    {
        builder.ToTable("Tournaments");
        
        builder.Property(t => t.Name)
            .IsRequired()
            .HasMaxLength(100);
        
        builder.Property(t => t.Description)
            .HasMaxLength(2000);
        
        builder.HasMany(t => t.TeamsParticipations)
            .WithOne(t => t.Tournament)
            .HasForeignKey(t => t.TournamentId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}