namespace OspreyPulseAPI.Modules.Competitions.Domain;

/// <summary>
/// A single game/match. ExternalId = API-Sports game id.
/// Maps to: competitions
/// </summary>
public class Competition
{
    public int Id { get; set; }
    public int SeasonId { get; set; }
    public Season Season { get; set; } = null!;

    /// <summary>API-Sports game id. Unique.</summary>
    public string? ExternalId { get; set; }
    public int HomeTeamId { get; set; }
    public Team HomeTeam { get; set; } = null!;
    public int AwayTeamId { get; set; }
    public Team AwayTeam { get; set; } = null!;

    public DateTimeOffset? StartTime { get; set; }
    public string? Venue { get; set; }
    public string? City { get; set; }
    public CompetitionStatus Status { get; set; }
    public int HomeScore { get; set; }
    public int AwayScore { get; set; }
    public short? CurrentPeriod { get; set; }
    public string? TimeRemaining { get; set; }

    /// <summary>Sport-specific data (JSONB).</summary>
    public string? Metadata { get; set; }

    public ICollection<CompetitionRoster> Rosters { get; set; } = new List<CompetitionRoster>();
}
