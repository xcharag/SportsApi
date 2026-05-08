using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SportsApi.domain.Modules.Teams;

namespace SportsApi.infrastructure.Persistence.Core.EntityFramework.Configurations;

public class PlayerConfiguration : BaseEntityConfiguration<Player>
{
    protected override void ConfigureEntity(EntityTypeBuilder<Player> builder)
    {
        builder.ToTable("Players");
        
        builder.Property(p => p.FullName)
            .IsRequired()
            .HasMaxLength(300);
        
        builder.Property(p => p.Ci)
            .HasMaxLength(20);
        
        builder.Property(p => p.PhoneNumber)
            .HasMaxLength(20);
        
        builder.HasMany(p => p.Rosters)
            .WithOne(r => r.Player)
            .HasForeignKey(r => r.PlayerId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}