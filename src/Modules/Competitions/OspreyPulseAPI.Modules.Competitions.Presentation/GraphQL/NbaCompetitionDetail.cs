namespace OspreyPulseAPI.Modules.Competitions.Presentation.GraphQL;

/// <summary>
/// GraphQL shape for NBA competition by event: teams with nested rosters (playerId, fullName, avatarUrl, jerseyNumber).
/// </summary>
public class NbaCompetitionDetail
{
    public string? ExternalId { get; set; }
    public DateTimeOffset? StartTime { get; set; }
    public int HomeScore { get; set; }
    public int AwayScore { get; set; }
    public NbaCompetitionTeam HomeTeam { get; set; } = null!;
    public NbaCompetitionTeam AwayTeam { get; set; } = null!;
}

/// <summary>
/// Team with nested rosters for a competition.
/// </summary>
public class NbaCompetitionTeam
{
    public int TeamId { get; set; }
    public string Name { get; set; } = null!;
    public string? Nickname { get; set; }
    public string? Code { get; set; }
    public string? City { get; set; }
    public string? LogoUrl { get; set; }
    public List<NbaRosterPlayer> Rosters { get; set; } = new();
}

/// <summary>
/// Roster entry: player id, fullName, avatarUrl, jerseyNumber.
/// </summary>
public class NbaRosterPlayer
{
    public int PlayerId { get; set; }
    public string FullName { get; set; } = null!;
    public string? AvatarUrl { get; set; }
    public short? JerseyNumber { get; set; }
}
