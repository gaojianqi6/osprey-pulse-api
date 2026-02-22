namespace OspreyPulseAPI.Modules.Competitions.Domain;

/// <summary>
/// A single game/match. Maps to: competitions
/// </summary>
public class Competition
{
    public int Id { get; set; }
    public int SeasonId { get; set; }
    public Season Season { get; set; } = null!;

    public int HomeTeamId { get; set; }
    public Team HomeTeam { get; set; } = null!;
    public int AwayTeamId { get; set; }
    public Team AwayTeam { get; set; } = null!;

    public DateTimeOffset? StartTime { get; set; }
    public CompetitionStatus Status { get; set; }
    public int HomeScore { get; set; }
    public int AwayScore { get; set; }

    /// <summary>League-specific: e.g. Map Name (LoL), Venue (Rugby).</summary>
    public string? Metadata { get; set; }

    public ICollection<CompetitionRoster> Rosters { get; set; } = new List<CompetitionRoster>();
}
