using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SportsApi.domain.Modules.Matches;

namespace SportsApi.infrastructure.Persistence.Core.EntityFramework.Configurations;

public class EventConfiguration : BaseEntityConfiguration<Event>
{
    protected override void ConfigureEntity(EntityTypeBuilder<Event> builder)
    {
        builder.ToTable("Events");
        
        builder.HasOne(e => e.Match)
            .WithMany(m => m.Events)
            .HasForeignKey(e => e.MatchId)
            .OnDelete(DeleteBehavior.Restrict);
        
        builder.HasOne(e => e.Roster)
            .WithMany(r => r.Events)
            .HasForeignKey(e => e.RosterId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}