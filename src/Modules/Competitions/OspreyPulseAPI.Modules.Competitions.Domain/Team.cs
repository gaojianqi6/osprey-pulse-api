namespace OspreyPulseAPI.Modules.Competitions.Domain;

/// <summary>
/// Team within a league.
/// Maps to: teams
/// </summary>
public class Team
{
    public int Id { get; set; }
    public int LeagueId { get; set; }
    public League League { get; set; } = null!;

    public required string Name { get; set; }
    public string? LogoUrl { get; set; }
    public string? Description { get; set; }

    public ICollection<Competition> HomeCompetitions { get; set; } = new List<Competition>();
    public ICollection<Competition> AwayCompetitions { get; set; } = new List<Competition>();
    public ICollection<CompetitionRoster> CompetitionRosters { get; set; } = new List<CompetitionRoster>();
    public ICollection<PlayerTeamAssignment> PlayerAssignments { get; set; } = new List<PlayerTeamAssignment>();
}
