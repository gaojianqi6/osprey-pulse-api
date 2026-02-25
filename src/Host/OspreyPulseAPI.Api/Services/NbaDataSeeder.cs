using Microsoft.EntityFrameworkCore;
using OspreyPulseAPI.Modules.Competitions.Application;
using OspreyPulseAPI.Modules.Competitions.Domain;
using OspreyPulseAPI.Modules.Competitions.Infrastructure.Persistence;

namespace OspreyPulseAPI.Api.Services;

/// <summary>
/// Seeds Channel (NBA), Leagues and Seasons from API-Sports NBA v2 on startup when data is empty.
/// </summary>
public class NbaDataSeeder : IHostedService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<NbaDataSeeder> _logger;

    public NbaDataSeeder(IServiceScopeFactory scopeFactory, ILogger<NbaDataSeeder> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        try
        {
            await using var scope = _scopeFactory.CreateAsyncScope();
            var db = scope.ServiceProvider.GetRequiredService<CompetitionsDbContext>();
            var espnIngestion = scope.ServiceProvider.GetRequiredService<IEspnNbaIngestionService>();

            // Ensure NBA channel exists
            var channel = await db.Channels.SingleOrDefaultAsync(c => c.Slug == "nba", cancellationToken);
            if (channel == null)
            {
                channel = new Channel
                {
                    Name = "NBA",
                    Slug = "nba",
                    SportType = "basketball",
                    Description = "National Basketball Association"
                };
                db.Channels.Add(channel);
                await db.SaveChangesAsync(cancellationToken);
            }

            _logger.LogInformation("Ensuring static NBA leagues and seasons...");

            // Ensure single NBA league under this channel
            var league = await db.Leagues
                .FirstOrDefaultAsync(l => l.ChannelId == channel.Id && l.Name == "NBA", cancellationToken);

            if (league == null)
            {
                league = new League
                {
                    ChannelId = channel.Id,
                    Name = "NBA",
                    ShortCode = "NBA"
                };
                db.Leagues.Add(league);
                await db.SaveChangesAsync(cancellationToken);
            }

            // Static seasons: label + optional date range
            var seasons = new (string Label, DateOnly? Start, DateOnly? End, bool IsCurrent)[]
            {
                ("2025 Preseason", new DateOnly(2025, 10, 2), new DateOnly(2025, 10, 17), false),
                ("2025-26 Regular Season", new DateOnly(2025, 10, 21), new DateOnly(2026, 04, 12), true),
                ("2026 Play-In Tournament", new DateOnly(2026, 04, 14), new DateOnly(2026, 04, 17), false),
                ("2026 Playoffs", new DateOnly(2026, 04, 18), new DateOnly(2026, 05, 31), false),
                ("2026 NBA Finals", new DateOnly(2026, 06, 02), new DateOnly(2026, 06, 19), false),
                ("2026 Summer League", new DateOnly(2026, 07, 07), new DateOnly(2026, 07, 17), false)
            };

            foreach (var (label, start, end, isCurrent) in seasons)
            {
                var exists = await db.Seasons
                    .AnyAsync(s => s.LeagueId == league.Id && s.Label == label, cancellationToken);
                if (exists)
                {
                    continue;
                }

                db.Seasons.Add(new Season
                {
                    LeagueId = league.Id,
                    Label = label,
                    StartDate = start,
                    EndDate = end,
                    IsCurrent = isCurrent
                });
            }

            await db.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("NBA static data ensured: channel 'nba', league 'NBA', {SeasonCount} seasons.",
                seasons.Length);

            // 2. Load/refresh NBA teams from ESPN only when we don't have any yet
            var hasTeams = await db.Teams.AnyAsync(t => t.LeagueId == league.Id, cancellationToken);
            if (!hasTeams)
            {
                _logger.LogInformation("Syncing NBA teams from ESPN...");
                await espnIngestion.EnsureTeamsAsync(cancellationToken);
            }
            else
            {
                _logger.LogDebug("NBA teams already present; skipping ESPN team sync.");
            }

            // 3. Load 3-day competitions (yesterday, today, tomorrow) — always run to refresh scores/status
            _logger.LogInformation("Syncing NBA competitions (3-day scoreboard) from ESPN...");
            await espnIngestion.EnsureUpcomingThreeDayScoreboardAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "NBA data seed failed.");
            throw;
        }
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
