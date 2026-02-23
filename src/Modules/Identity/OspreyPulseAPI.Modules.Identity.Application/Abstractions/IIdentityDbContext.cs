using Microsoft.EntityFrameworkCore;
using OspreyPulseAPI.Modules.Identity.Domain;

namespace OspreyPulseAPI.Modules.Identity.Application.Abstractions;

/// <summary>
/// Identity data access contract. Implemented by Infrastructure; used by Application handlers.
/// Keeps Application independent of EF Core and persistence details.
/// </summary>
public interface IIdentityDbContext
{
    DbSet<User> Users { get; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
