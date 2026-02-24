namespace OspreyPulseAPI.Modules.Competitions.Domain;

/// <summary>
/// A player/coach/referee in a specific game. Stats (JSONB) are sport-specific.
/// Maps to: competition_rosters
/// </summary>
public class CompetitionRoster
{
    public int Id { get; set; }
    public int CompetitionId { get; set; }
    public Competition Competition { get; set; } = null!;

    /// <summary>Nullable for coaches/referees without a player record.</summary>
    public int? PlayerId { get; set; }
    public Player? Player { get; set; }

    public int TeamId { get; set; }
    public Team Team { get; set; } = null!;

    public RosterRoleType? RoleType { get; set; }
    public LineupStatus? LineupStatus { get; set; }
    public string? PositionPlayed { get; set; }
    public string? MinutesPlayed { get; set; }

    /// <summary>Sport-specific stats (e.g. points, kills). Stored as JSONB.</summary>
    public string? Stats { get; set; }

    public decimal RatingAvg { get; set; }
}
