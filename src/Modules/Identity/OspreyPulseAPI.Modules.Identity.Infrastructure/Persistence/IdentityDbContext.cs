using Microsoft.EntityFrameworkCore;
using OspreyPulseAPI.Modules.Identity.Application.Abstractions;
using OspreyPulseAPI.Modules.Identity.Domain;

namespace OspreyPulseAPI.Modules.Identity.Infrastructure.Persistence;

public class IdentityDbContext : DbContext, IIdentityDbContext
{
    public IdentityDbContext(DbContextOptions<IdentityDbContext> options)
        : base(options) { }

    public DbSet<User> Users => Set<User>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("identity");
        base.OnModelCreating(modelBuilder);
    }
}
