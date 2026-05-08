using Microsoft.EntityFrameworkCore;
using SportsApi.domain.Modules.Matches;
using SportsApi.domain.Modules.Teams;
using SportsApi.domain.Modules.Tournaments;

namespace SportsApi.infrastructure.Persistence.Core.EntityFramework;

public class CoreDbContext(DbContextOptions<CoreDbContext> options) : DbContext(options)
{
    public DbSet<Tournament> Tournaments { get; set; }
    public DbSet<Team> Teams { get; set; }
    public DbSet<Roster> Rosters { get; set; }
    public DbSet<Match> Matches { get; set; }
    public DbSet<Event> Events { get; set; }
    public DbSet<TeamParticipation> TeamParticipations { get; set; }
    public DbSet<Player> Players { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(CoreDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}