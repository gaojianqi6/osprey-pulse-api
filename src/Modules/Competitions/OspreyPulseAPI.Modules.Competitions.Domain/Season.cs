namespace OspreyPulseAPI.Modules.Competitions.Domain;

/// <summary>
/// A season within a league. Uses a human-readable label plus optional start/end dates.
/// Maps to: seasons
/// </summary>
public class Season
{
    public int Id { get; set; }
    public int LeagueId { get; set; }
    public League League { get; set; } = null!;

    /// <summary>e.g. '2025 Preseason', '2025-26 Regular Season'. Unique per league.</summary>
    public required string Label { get; set; }
    public DateOnly? StartDate { get; set; }
    public DateOnly? EndDate { get; set; }
    public bool IsCurrent { get; set; }

    public ICollection<Competition> Competitions { get; set; } = new List<Competition>();
}
