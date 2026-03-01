using Microsoft.EntityFrameworkCore;
using OspreyPulseAPI.Modules.Competitions.Domain;

namespace OspreyPulseAPI.Modules.Competitions.Infrastructure.Persistence;

public class CompetitionsDbContext : DbContext
{
    public CompetitionsDbContext(DbContextOptions<CompetitionsDbContext> options)
        : base(options) { }

    public DbSet<Channel> Channels => Set<Channel>();
    public DbSet<League> Leagues => Set<League>();
    public DbSet<Season> Seasons => Set<Season>();
    public DbSet<Team> Teams => Set<Team>();
    public DbSet<Player> Players => Set<Player>();
    public DbSet<PlayerTeamAssignment> PlayerTeamAssignments => Set<PlayerTeamAssignment>();
    public DbSet<Competition> Competitions => Set<Competition>();
    public DbSet<CompetitionRoster> CompetitionRosters => Set<CompetitionRoster>();
    public DbSet<Post> Posts => Set<Post>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("competitions");

        // Apply JSONB and composite-key configurations from EntityConfigurations
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(CompetitionsDbContext).Assembly);

        base.OnModelCreating(modelBuilder);
    }
}
