using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SportsApi.domain.Modules.Tournaments;

namespace SportsApi.infrastructure.Persistence.Core.EntityFramework.Configurations;

public class RoundsClassifiedConfiguration : BaseEntityConfiguration<RoundsClassified>
{
    protected override void ConfigureEntity(EntityTypeBuilder<RoundsClassified> builder)
    {
        builder.ToTable("RoundsClassified");

        builder.Property(r => r.RoundKey)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(r => r.NextRoundKey)
            .HasMaxLength(50);

        builder.Property(r => r.Round)
            .IsRequired();

        builder.HasOne(r => r.TeamParticipation)
            .WithMany(tp => tp.RoundsClassified)
            .HasForeignKey(r => r.TeamParticipationId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

