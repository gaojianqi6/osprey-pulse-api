namespace OspreyPulseAPI.Modules.Competitions.Domain;

/// <summary>
/// A season within a league. YearLabel = API-Sports YYYY or '2025-2026'.
/// Maps to: seasons
/// </summary>
public class Season
{
    public int Id { get; set; }
    public int LeagueId { get; set; }
    public League League { get; set; } = null!;

    /// <summary>e.g. '2025', '2025-2026'. Unique per league.</summary>
    public required string YearLabel { get; set; }
    public bool IsCurrent { get; set; }

    public ICollection<Competition> Competitions { get; set; } = new List<Competition>();
}
