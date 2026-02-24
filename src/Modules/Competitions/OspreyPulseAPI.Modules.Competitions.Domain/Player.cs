namespace OspreyPulseAPI.Modules.Competitions.Domain;

/// <summary>
/// Player (or coach/referee) - shared across sports. ExternalId = API-Sports player id.
/// Maps to: players
/// </summary>
public class Player
{
    public int Id { get; set; }
    /// <summary>API-Sports player id. Unique.</summary>
    public string? ExternalId { get; set; }
    public required string FullName { get; set; }
    public string? Nationality { get; set; }
    public DateOnly? BirthDate { get; set; }
    public short? HeightCm { get; set; }
    public short? WeightKg { get; set; }
    public string? AvatarUrl { get; set; }
    public string? DefaultPosition { get; set; }

    public ICollection<CompetitionRoster> CompetitionRosters { get; set; } = new List<CompetitionRoster>();
    public ICollection<PlayerTeamAssignment> TeamAssignments { get; set; } = new List<PlayerTeamAssignment>();
}
