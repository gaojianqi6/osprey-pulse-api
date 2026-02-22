namespace OspreyPulseAPI.Modules.Competitions.Domain;

/// <summary>
/// Player (or coach/referee) - shared across sports; sport-specific data in competition_rosters.stats.
/// Maps to: players
/// </summary>
public class Player
{
    public int Id { get; set; }
    public required string FullName { get; set; }
    public string? Nationality { get; set; }
    public string? AvatarUrl { get; set; }
    public string? DefaultPosition { get; set; }

    public ICollection<CompetitionRoster> CompetitionRosters { get; set; } = new List<CompetitionRoster>();
    public ICollection<PlayerTeamAssignment> TeamAssignments { get; set; } = new List<PlayerTeamAssignment>();
}
