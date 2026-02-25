using HotChocolate;
using HotChocolate.Types;
using Microsoft.EntityFrameworkCore;
using OspreyPulseAPI.Modules.Competitions.Application;
using OspreyPulseAPI.Modules.Competitions.Domain;
using OspreyPulseAPI.Modules.Competitions.Infrastructure.Persistence;

namespace OspreyPulseAPI.Modules.Competitions.Presentation.GraphQL;

[ExtendObjectType("Query")]
public class CompetitionsQueries
{
    /// <summary>
    /// List all NBA teams.
    /// </summary>
    public async Task<List<Team>> NbaTeams(
        [Service] CompetitionsDbContext db,
        CancellationToken cancellationToken = default)
    {
        return await db.Teams
            .Include(t => t.League)
            .ThenInclude(l => l.Channel)
            .Where(t => t.League.Channel.Slug == "nba")
            .OrderBy(t => t.Name)
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Today's NBA competitions (by UTC date).
    /// </summary>
    public async Task<List<Competition>> NbaTodayCompetitions(
        [Service] CompetitionsDbContext db,
        CancellationToken cancellationToken = default)
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow.Date);
        var tomorrow = today.AddDays(1);

        return await db.Competitions
            .Include(c => c.Season)
            .ThenInclude(s => s.League)
            .ThenInclude(l => l.Channel)
            .Include(c => c.HomeTeam)
            .Include(c => c.AwayTeam)
            .Where(c =>
                c.Season.League.Channel.Slug == "nba" &&
                c.StartTime.HasValue &&
                DateOnly.FromDateTime(c.StartTime.Value.UtcDateTime) >= today &&
                DateOnly.FromDateTime(c.StartTime.Value.UtcDateTime) < tomorrow)
            .OrderBy(c => c.StartTime)
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Recently completed NBA competitions (default last 3 days, UTC).
    /// </summary>
    public async Task<List<Competition>> NbaRecentCompletedCompetitions(
        [Service] CompetitionsDbContext db,
        int days = 3,
        CancellationToken cancellationToken = default)
    {
        if (days < 1)
        {
            days = 1;
        }

        var now = DateTime.UtcNow;
        var from = now.AddDays(-days);

        return await db.Competitions
            .Include(c => c.Season)
            .ThenInclude(s => s.League)
            .ThenInclude(l => l.Channel)
            .Include(c => c.HomeTeam)
            .Include(c => c.AwayTeam)
            .Where(c =>
                c.Season.League.Channel.Slug == "nba" &&
                c.Status == CompetitionStatus.Finished &&
                c.StartTime.HasValue &&
                c.StartTime.Value >= from &&
                c.StartTime.Value <= now)
            .OrderByDescending(c => c.StartTime)
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Single NBA competition by ESPN event id. Triggers ESPN summary/roster sync on demand.
    /// </summary>
    public async Task<Competition?> NbaCompetitionByEventId(
        string eventId,
        [Service] CompetitionsDbContext db,
        [Service] IEspnNbaIngestionService ingestion,
        CancellationToken cancellationToken = default)
    {
        await ingestion.EnsureCompetitionDetailsAsync(eventId, cancellationToken);

        return await db.Competitions
            .Include(c => c.Season)
            .ThenInclude(s => s.League)
            .ThenInclude(l => l.Channel)
            .Include(c => c.HomeTeam)
            .Include(c => c.AwayTeam)
            .Include(c => c.Rosters)
            .ThenInclude(r => r.Player)
            .ThenInclude(p => p.TeamAssignments)
            .AsSplitQuery()
            .FirstOrDefaultAsync(c => c.ExternalId == eventId, cancellationToken);
    }
}

