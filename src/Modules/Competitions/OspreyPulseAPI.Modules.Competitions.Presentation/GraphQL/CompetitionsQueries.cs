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
    /// Returns structured homeTeam/awayTeam with nested rosters (playerId, fullName, avatarUrl, jerseyNumber).
    /// </summary>
    public async Task<NbaCompetitionDetail?> NbaCompetitionByEventId(
        string eventId,
        [Service] CompetitionsDbContext db,
        [Service] IEspnNbaIngestionService ingestion,
        CancellationToken cancellationToken = default)
    {
        await ingestion.EnsureCompetitionDetailsAsync(eventId, cancellationToken);

        var competition = await db.Competitions
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

        return competition == null ? null : MapToNbaCompetitionDetail(competition);
    }

    private static NbaCompetitionDetail MapToNbaCompetitionDetail(Competition c)
    {
        return new NbaCompetitionDetail
        {
            ExternalId = c.ExternalId,
            StartTime = c.StartTime,
            HomeScore = c.HomeScore,
            AwayScore = c.AwayScore,
            HomeTeam = MapToNbaCompetitionTeam(c.HomeTeam, c.Rosters.Where(r => r.TeamId == c.HomeTeamId).ToList()),
            AwayTeam = MapToNbaCompetitionTeam(c.AwayTeam, c.Rosters.Where(r => r.TeamId == c.AwayTeamId).ToList())
        };
    }

    private static NbaCompetitionTeam MapToNbaCompetitionTeam(Team team, List<CompetitionRoster> rosters)
    {
        return new NbaCompetitionTeam
        {
            TeamId = team.Id,
            Name = team.Name,
            Nickname = team.Nickname,
            Code = team.Code,
            City = team.City,
            LogoUrl = team.LogoUrl,
            Rosters = rosters
                .Where(r => r.Player != null)
                .Select(r => new NbaRosterPlayer
                {
                    PlayerId = r.Player!.Id,
                    FullName = r.Player.FullName,
                    AvatarUrl = r.Player.AvatarUrl,
                    JerseyNumber = r.Player.TeamAssignments
                        .FirstOrDefault(a => a.TeamId == team.Id && a.IsActive)?.JerseyNumber
                })
                .ToList()
        };
    }
}

