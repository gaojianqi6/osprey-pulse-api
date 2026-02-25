using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using OspreyPulseAPI.Modules.Competitions.Application;
using OspreyPulseAPI.Modules.Competitions.Domain;
using OspreyPulseAPI.Modules.Competitions.Infrastructure.Persistence;

namespace OspreyPulseAPI.Api.Services;

public class EspnNbaIngestionService : IEspnNbaIngestionService
{
    private readonly CompetitionsDbContext _db;
    private readonly IEspnNbaClient _client;
    private readonly ILogger<EspnNbaIngestionService> _logger;

    public EspnNbaIngestionService(
        CompetitionsDbContext db,
        IEspnNbaClient client,
        ILogger<EspnNbaIngestionService> logger)
    {
        _db = db;
        _client = client;
        _logger = logger;
    }

    public async Task EnsureTeamsAsync(CancellationToken cancellationToken = default)
    {
        var league = await GetNbaLeagueAsync(cancellationToken);
        if (league == null)
        {
            _logger.LogWarning("NBA league not found; skipping ESPN team sync.");
            return;
        }

        using var document = await _client.GetTeamsAsync(cancellationToken);
        var root = document.RootElement;

        if (!root.TryGetProperty("sports", out var sportsArray) || sportsArray.GetArrayLength() == 0)
        {
            _logger.LogWarning("ESPN teams response missing 'sports' collection.");
            return;
        }

        var leaguesArray = sportsArray[0].GetProperty("leagues");
        if (leaguesArray.GetArrayLength() == 0)
        {
            _logger.LogWarning("ESPN teams response missing 'leagues' collection.");
            return;
        }

        var teamsArray = leaguesArray[0].GetProperty("teams");
        foreach (var teamWrapper in teamsArray.EnumerateArray())
        {
            if (!teamWrapper.TryGetProperty("team", out var teamElement))
            {
                continue;
            }

            var espnId = teamElement.GetProperty("id").GetString();
            if (string.IsNullOrWhiteSpace(espnId))
            {
                continue;
            }

            var name = teamElement.GetProperty("displayName").GetString() ?? teamElement.GetProperty("name").GetString();
            if (string.IsNullOrWhiteSpace(name))
            {
                continue;
            }

            var location = teamElement.TryGetProperty("location", out var locationEl) ? locationEl.GetString() : null;
            var nickname = teamElement.TryGetProperty("shortDisplayName", out var shortNameEl)
                ? shortNameEl.GetString()
                : teamElement.TryGetProperty("nickname", out var nicknameEl)
                    ? nicknameEl.GetString()
                    : null;
            var abbreviation = teamElement.TryGetProperty("abbreviation", out var abbrEl) ? abbrEl.GetString() : null;

            string? logoUrl = null;
            if (teamElement.TryGetProperty("logos", out var logosEl) && logosEl.ValueKind == JsonValueKind.Array &&
                logosEl.GetArrayLength() > 0)
            {
                logoUrl = logosEl[0].TryGetProperty("href", out var hrefEl) ? hrefEl.GetString() : null;
            }

            var existing = await _db.Teams
                .FirstOrDefaultAsync(t => t.ExternalId == espnId && t.LeagueId == league.Id, cancellationToken);

            if (existing == null)
            {
                existing = new Team
                {
                    LeagueId = league.Id,
                    ExternalId = espnId,
                    Name = name,
                    Nickname = nickname,
                    Code = abbreviation,
                    City = location,
                    LogoUrl = logoUrl
                };
                _db.Teams.Add(existing);
            }
            else
            {
                existing.Name = name;
                existing.Nickname = nickname;
                existing.Code = abbreviation;
                existing.City = location;
                existing.LogoUrl = logoUrl;
            }
        }

        await _db.SaveChangesAsync(cancellationToken);
    }

    public async Task EnsureUpcomingThreeDayScoreboardAsync(CancellationToken cancellationToken = default)
    {
        var league = await GetNbaLeagueAsync(cancellationToken);
        if (league == null)
        {
            _logger.LogWarning("NBA league not found; skipping ESPN scoreboard sync.");
            return;
        }

        var season = await _db.Seasons
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.LeagueId == league.Id && s.IsCurrent, cancellationToken);

        if (season == null)
        {
            _logger.LogWarning("NBA current season not found; skipping ESPN scoreboard sync.");
            return;
        }

        var today = DateOnly.FromDateTime(DateTime.UtcNow.Date);
        var from = today.AddDays(-1);
        var to = today.AddDays(1);

        using var document = await _client.GetScoreboardAsync(from, to, cancellationToken);
        var root = document.RootElement;

        if (!root.TryGetProperty("events", out var eventsArray) || eventsArray.ValueKind != JsonValueKind.Array)
        {
            _logger.LogInformation("ESPN scoreboard response has no events for range {From} - {To}", from, to);
            return;
        }

        foreach (var ev in eventsArray.EnumerateArray())
        {
            var eventId = ev.TryGetProperty("id", out var idEl) ? idEl.GetString() : null;
            if (string.IsNullOrWhiteSpace(eventId))
            {
                continue;
            }

            var competitionsArray = ev.TryGetProperty("competitions", out var compsEl) &&
                                    compsEl.ValueKind == JsonValueKind.Array &&
                                    compsEl.GetArrayLength() > 0
                ? compsEl
                : default;

            if (competitionsArray.ValueKind != JsonValueKind.Array || competitionsArray.GetArrayLength() == 0)
            {
                continue;
            }

            var compEl = competitionsArray[0];

            DateTimeOffset? startTime = null;
            if (compEl.TryGetProperty("date", out var dateEl) || ev.TryGetProperty("date", out dateEl))
            {
                if (DateTimeOffset.TryParse(dateEl.GetString(), out var parsed))
                {
                    startTime = parsed;
                }
            }

            string? venueName = null;
            string? city = null;
            if (compEl.TryGetProperty("venue", out var venueEl))
            {
                venueName = venueEl.TryGetProperty("fullName", out var fullNameEl) ? fullNameEl.GetString() : null;
                if (venueEl.TryGetProperty("address", out var addrEl))
                {
                    city = addrEl.TryGetProperty("city", out var cityEl) ? cityEl.GetString() : null;
                }
            }

            short? period = null;
            string? clock = null;
            var status = CompetitionStatus.NotStarted;

            if (ev.TryGetProperty("status", out var statusEl) || compEl.TryGetProperty("status", out statusEl))
            {
                if (statusEl.TryGetProperty("period", out var periodEl) && periodEl.TryGetInt32(out var periodInt))
                {
                    period = (short)periodInt;
                }

                if (statusEl.TryGetProperty("displayClock", out var clockEl))
                {
                    clock = clockEl.GetString();
                }

                if (statusEl.TryGetProperty("type", out var typeEl) &&
                    typeEl.TryGetProperty("state", out var stateEl))
                {
                    var state = stateEl.GetString();
                    status = state switch
                    {
                        "pre" => CompetitionStatus.NotStarted,
                        "in" => CompetitionStatus.Live,
                        "post" => CompetitionStatus.Finished,
                        "postponed" => CompetitionStatus.Postponed,
                        "canceled" => CompetitionStatus.Canceled,
                        _ => CompetitionStatus.NotStarted
                    };
                }
            }

            int homeScore = 0;
            int awayScore = 0;
            int? homeTeamId = null;
            int? awayTeamId = null;

            if (compEl.TryGetProperty("competitors", out var competitorsEl) &&
                competitorsEl.ValueKind == JsonValueKind.Array)
            {
                foreach (var competitor in competitorsEl.EnumerateArray())
                {
                    if (!competitor.TryGetProperty("team", out var teamEl))
                    {
                        continue;
                    }

                    var espnTeamId = teamEl.TryGetProperty("id", out var teamIdEl) ? teamIdEl.GetString() : null;
                    if (string.IsNullOrWhiteSpace(espnTeamId))
                    {
                        continue;
                    }

                    var dbTeam = await EnsureTeamFromEspnTeamElementAsync(
                        league.Id,
                        teamEl,
                        cancellationToken);

                    if (!competitor.TryGetProperty("homeAway", out var sideEl))
                    {
                        continue;
                    }

                    var side = sideEl.GetString();
                    var score = 0;
                    if (competitor.TryGetProperty("score", out var scoreEl) &&
                        int.TryParse(scoreEl.GetString(), out var parsedScore))
                    {
                        score = parsedScore;
                    }

                    if (string.Equals(side, "home", StringComparison.OrdinalIgnoreCase))
                    {
                        homeTeamId = dbTeam.Id;
                        homeScore = score;
                    }
                    else if (string.Equals(side, "away", StringComparison.OrdinalIgnoreCase))
                    {
                        awayTeamId = dbTeam.Id;
                        awayScore = score;
                    }
                }
            }

            if (homeTeamId == null || awayTeamId == null)
            {
                continue;
            }

            var competition = await _db.Competitions
                .FirstOrDefaultAsync(c => c.ExternalId == eventId, cancellationToken);

            if (competition == null)
            {
                competition = new Competition
                {
                    SeasonId = season.Id,
                    ExternalId = eventId,
                    HomeTeamId = homeTeamId.Value,
                    AwayTeamId = awayTeamId.Value,
                    StartTime = startTime,
                    Venue = venueName,
                    City = city,
                    Status = status,
                    HomeScore = homeScore,
                    AwayScore = awayScore,
                    CurrentPeriod = period,
                    TimeRemaining = clock,
                    Metadata = ev.GetRawText()
                };
                _db.Competitions.Add(competition);
            }
            else
            {
                competition.SeasonId = season.Id;
                competition.HomeTeamId = homeTeamId.Value;
                competition.AwayTeamId = awayTeamId.Value;
                competition.StartTime = startTime;
                competition.Venue = venueName;
                competition.City = city;
                competition.Status = status;
                competition.HomeScore = homeScore;
                competition.AwayScore = awayScore;
                competition.CurrentPeriod = period;
                competition.TimeRemaining = clock;
                competition.Metadata = ev.GetRawText();
            }
        }

        await _db.SaveChangesAsync(cancellationToken);
    }

    public async Task EnsureCompetitionDetailsAsync(
        string eventId,
        CancellationToken cancellationToken = default)
    {
        var competition = await _db.Competitions
            .Include(c => c.HomeTeam)
            .Include(c => c.AwayTeam)
            .FirstOrDefaultAsync(c => c.ExternalId == eventId, cancellationToken);

        if (competition == null)
        {
            _logger.LogWarning("Competition with ESPN event id {EventId} not found in DB.", eventId);
            return;
        }

        // Finished/canceled/postponed events won't change; skip ESPN and use stored data
        if (competition.Status == CompetitionStatus.Finished ||
            competition.Status == CompetitionStatus.Canceled ||
            competition.Status == CompetitionStatus.Postponed)
        {
            return;
        }

        using var document = await _client.GetEventSummaryAsync(eventId, cancellationToken);
        var root = document.RootElement;

        competition.Metadata = root.GetRawText();

        // Optionally refresh scores/status from summary
        if (root.TryGetProperty("header", out var headerEl) &&
            headerEl.TryGetProperty("competitions", out var compsEl) &&
            compsEl.ValueKind == JsonValueKind.Array &&
            compsEl.GetArrayLength() > 0)
        {
            var compEl = compsEl[0];

            if (compEl.TryGetProperty("status", out var statusEl))
            {
                if (statusEl.TryGetProperty("period", out var periodEl) &&
                    periodEl.TryGetInt32(out var periodInt))
                {
                    competition.CurrentPeriod = (short)periodInt;
                }

                if (statusEl.TryGetProperty("displayClock", out var clockEl))
                {
                    competition.TimeRemaining = clockEl.GetString();
                }

                if (statusEl.TryGetProperty("type", out var typeEl) &&
                    typeEl.TryGetProperty("state", out var stateEl))
                {
                    var state = stateEl.GetString();
                    competition.Status = state switch
                    {
                        "pre" => CompetitionStatus.NotStarted,
                        "in" => CompetitionStatus.Live,
                        "post" => CompetitionStatus.Finished,
                        "postponed" => CompetitionStatus.Postponed,
                        "canceled" => CompetitionStatus.Canceled,
                        _ => competition.Status
                    };
                }
            }

            if (compEl.TryGetProperty("competitors", out var competitorsEl) &&
                competitorsEl.ValueKind == JsonValueKind.Array)
            {
                foreach (var competitor in competitorsEl.EnumerateArray())
                {
                    if (!competitor.TryGetProperty("team", out var teamEl))
                    {
                        continue;
                    }

                    var espnTeamId = teamEl.TryGetProperty("id", out var teamIdEl) ? teamIdEl.GetString() : null;
                    if (string.IsNullOrWhiteSpace(espnTeamId))
                    {
                        continue;
                    }

                    var score = 0;
                    if (competitor.TryGetProperty("score", out var scoreEl) &&
                        int.TryParse(scoreEl.GetString(), out var parsedScore))
                    {
                        score = parsedScore;
                    }

                    if (!competitor.TryGetProperty("homeAway", out var sideEl))
                    {
                        continue;
                    }

                    var side = sideEl.GetString();
                    if (string.Equals(side, "home", StringComparison.OrdinalIgnoreCase))
                    {
                        competition.HomeScore = score;
                    }
                    else if (string.Equals(side, "away", StringComparison.OrdinalIgnoreCase))
                    {
                        competition.AwayScore = score;
                    }
                }
            }
        }

        // Only fetch team rosters from ESPN if we don't already have rosters for this competition
        var hasRosters = await _db.CompetitionRosters
            .AnyAsync(r => r.CompetitionId == competition.Id, cancellationToken);
        if (!hasRosters)
        {
            await EnsureTeamPlayersIfMissingAsync(competition.HomeTeam, cancellationToken);
            await EnsureTeamPlayersIfMissingAsync(competition.AwayTeam, cancellationToken);
        }

        await EnsureCompetitionRostersAsync(competition, cancellationToken);

        await _db.SaveChangesAsync(cancellationToken);
    }

    /// <summary>
    /// Ensures competition_rosters has an entry for each current team member (home and away).
    /// Does not remove players; only adds missing roster entries.
    /// </summary>
    private async Task EnsureCompetitionRostersAsync(
        Competition competition,
        CancellationToken cancellationToken)
    {
        foreach (var teamId in new[] { competition.HomeTeamId, competition.AwayTeamId })
        {
            var assignments = await _db.PlayerTeamAssignments
                .Where(a => a.TeamId == teamId && a.IsActive)
                .Select(a => new { a.PlayerId })
                .ToListAsync(cancellationToken);

            foreach (var a in assignments)
            {
                var exists = await _db.CompetitionRosters
                    .AnyAsync(r => r.CompetitionId == competition.Id && r.TeamId == teamId && r.PlayerId == a.PlayerId, cancellationToken);
                if (exists)
                {
                    continue;
                }

                _db.CompetitionRosters.Add(new CompetitionRoster
                {
                    CompetitionId = competition.Id,
                    TeamId = teamId,
                    PlayerId = a.PlayerId
                });
            }
        }
    }

    private async Task<League?> GetNbaLeagueAsync(CancellationToken cancellationToken)
    {
        return await _db.Leagues
            .Include(l => l.Channel)
            .FirstOrDefaultAsync(
                l => l.Channel.Slug == "nba" && l.Name == "NBA",
                cancellationToken);
    }

    private async Task<Team> EnsureTeamFromEspnTeamElementAsync(
        int leagueId,
        JsonElement teamElement,
        CancellationToken cancellationToken)
    {
        var espnId = teamElement.TryGetProperty("id", out var idEl) ? idEl.GetString() : null;
        if (string.IsNullOrWhiteSpace(espnId))
        {
            throw new InvalidOperationException("ESPN team element missing id.");
        }

        var existing = await _db.Teams
            .FirstOrDefaultAsync(t => t.ExternalId == espnId && t.LeagueId == leagueId, cancellationToken);
        if (existing != null)
        {
            return existing;
        }

        var name = teamElement.TryGetProperty("displayName", out var nameEl)
            ? nameEl.GetString()
            : teamElement.TryGetProperty("name", out nameEl)
                ? nameEl.GetString()
                : espnId;

        var location = teamElement.TryGetProperty("location", out var locationEl) ? locationEl.GetString() : null;
        var nickname = teamElement.TryGetProperty("shortDisplayName", out var shortNameEl)
            ? shortNameEl.GetString()
            : teamElement.TryGetProperty("nickname", out var nicknameEl)
                ? nicknameEl.GetString()
                : null;
        var abbreviation = teamElement.TryGetProperty("abbreviation", out var abbrEl) ? abbrEl.GetString() : null;

        string? logoUrl = null;
        if (teamElement.TryGetProperty("logos", out var logosEl) &&
            logosEl.ValueKind == JsonValueKind.Array &&
            logosEl.GetArrayLength() > 0)
        {
            logoUrl = logosEl[0].TryGetProperty("href", out var hrefEl) ? hrefEl.GetString() : null;
        }

        var team = new Team
        {
            LeagueId = leagueId,
            ExternalId = espnId,
            Name = name ?? espnId,
            Nickname = nickname,
            Code = abbreviation,
            City = location,
            LogoUrl = logoUrl
        };

        _db.Teams.Add(team);
        await _db.SaveChangesAsync(cancellationToken);

        return team;
    }

    private async Task EnsureTeamPlayersIfMissingAsync(
        Team team,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(team.ExternalId))
        {
            return;
        }

        var hasPlayers = await _db.PlayerTeamAssignments
            .AnyAsync(a => a.TeamId == team.Id && a.IsActive, cancellationToken);

        if (hasPlayers)
        {
            return;
        }

        using var document = await _client.GetTeamRosterAsync(team.ExternalId, cancellationToken);
        var root = document.RootElement;

        if (!root.TryGetProperty("athletes", out var athletesArray) ||
            athletesArray.ValueKind != JsonValueKind.Array)
        {
            return;
        }

        // ESPN roster API returns athletes as a direct array of player objects (or array of groups with "items")
        foreach (var athleteEl in athletesArray.EnumerateArray())
        {
            // Support both shapes: direct athlete object, or group with "items" array
            JsonElement athlete;
            if (athleteEl.TryGetProperty("items", out var itemsEl) && itemsEl.ValueKind == JsonValueKind.Array)
            {
                foreach (var a in itemsEl.EnumerateArray())
                {
                    await ProcessRosterAthleteAsync(a, team, cancellationToken);
                }
                continue;
            }
            athlete = athleteEl;
            await ProcessRosterAthleteAsync(athlete, team, cancellationToken);
        }

        await _db.SaveChangesAsync(cancellationToken);
    }

    private async Task ProcessRosterAthleteAsync(
        JsonElement athlete,
        Team team,
        CancellationToken cancellationToken)
    {
        var espnPlayerId = athlete.TryGetProperty("id", out var idEl) ? idEl.GetString() : null;
        if (string.IsNullOrWhiteSpace(espnPlayerId))
        {
            return;
        }

        var fullName = athlete.TryGetProperty("displayName", out var nameEl)
            ? nameEl.GetString()
            : athlete.TryGetProperty("fullName", out nameEl)
                ? nameEl.GetString()
                : athlete.TryGetProperty("shortName", out nameEl)
                    ? nameEl.GetString()
                    : espnPlayerId;
        if (string.IsNullOrWhiteSpace(fullName))
        {
            fullName = espnPlayerId;
        }

        var player = await _db.Players
            .FirstOrDefaultAsync(p => p.ExternalId == espnPlayerId, cancellationToken);

        if (player == null)
        {
            player = new Player
            {
                ExternalId = espnPlayerId,
                FullName = fullName
            };

            if (athlete.TryGetProperty("dateOfBirth", out var dobEl))
            {
                var dobStr = dobEl.GetString();
                if (!string.IsNullOrEmpty(dobStr) && dobStr.Length >= 10 && DateOnly.TryParse(dobStr.AsSpan(0, 10), out var dob))
                {
                    player.BirthDate = dob;
                }
            }

            if (athlete.TryGetProperty("height", out var heightEl))
            {
                if (heightEl.TryGetDouble(out var heightInches))
                {
                    player.HeightCm = (short)Math.Round(heightInches * 2.54);
                }
            }

            if (athlete.TryGetProperty("weight", out var weightEl))
            {
                if (weightEl.TryGetDouble(out var weightLb))
                {
                    player.WeightKg = (short)Math.Round(weightLb * 0.45359237);
                }
            }

            if (athlete.TryGetProperty("headshot", out var headshotEl) &&
                headshotEl.TryGetProperty("href", out var hrefEl))
            {
                player.AvatarUrl = hrefEl.GetString();
            }

            if (athlete.TryGetProperty("position", out var positionEl) &&
                positionEl.TryGetProperty("abbreviation", out var posAbbrEl))
            {
                player.DefaultPosition = posAbbrEl.GetString();
            }

            _db.Players.Add(player);
            await _db.SaveChangesAsync(cancellationToken);
        }

        var assignment = await _db.PlayerTeamAssignments
            .FirstOrDefaultAsync(
                a => a.PlayerId == player.Id && a.TeamId == team.Id,
                cancellationToken);

        if (assignment == null)
        {
            assignment = new PlayerTeamAssignment
            {
                PlayerId = player.Id,
                TeamId = team.Id,
                IsActive = true
            };
            if (athlete.TryGetProperty("jersey", out var jerseyEl))
            {
                if (jerseyEl.ValueKind == JsonValueKind.Number && jerseyEl.TryGetInt32(out var jerseyInt))
                {
                    assignment.JerseyNumber = (short)jerseyInt;
                }
                else if (short.TryParse(jerseyEl.GetString(), out var jerseyShort))
                {
                    assignment.JerseyNumber = jerseyShort;
                }
            }
            _db.PlayerTeamAssignments.Add(assignment);
        }
        else
        {
            assignment.IsActive = true;
            if (athlete.TryGetProperty("jersey", out var jerseyEl))
            {
                if (jerseyEl.ValueKind == JsonValueKind.Number && jerseyEl.TryGetInt32(out var jerseyInt))
                {
                    assignment.JerseyNumber = (short)jerseyInt;
                }
                else if (short.TryParse(jerseyEl.GetString(), out var jerseyShort))
                {
                    assignment.JerseyNumber = jerseyShort;
                }
            }
        }
    }
}

