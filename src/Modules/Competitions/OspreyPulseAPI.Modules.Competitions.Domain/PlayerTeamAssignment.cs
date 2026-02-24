namespace OspreyPulseAPI.Modules.Competitions.Domain;

/// <summary>
/// Current team assignment for a player. Historical assignments are in competition_rosters.
/// Maps to: player_team_assignments (composite PK: player_id, team_id)
/// </summary>
public class PlayerTeamAssignment
{
    public int PlayerId { get; set; }
    public Player Player { get; set; } = null!;

    public int TeamId { get; set; }
    public Team Team { get; set; } = null!;

    public bool IsActive { get; set; } = true;
    public DateOnly? JoinedDate { get; set; }
    public short? JerseyNumber { get; set; }
}
