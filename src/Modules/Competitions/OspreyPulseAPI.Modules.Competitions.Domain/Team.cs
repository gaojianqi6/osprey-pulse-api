namespace OspreyPulseAPI.Modules.Competitions.Domain;

/// <summary>
/// Team within a league. ExternalId = API-Sports team id.
/// Maps to: teams
/// </summary>
public class Team
{
    public int Id { get; set; }
    public int LeagueId { get; set; }
    public League League { get; set; } = null!;

    /// <summary>API-Sports team id. Unique.</summary>
    public string? ExternalId { get; set; }
    public required string Name { get; set; }
    public string? Nickname { get; set; }
    /// <summary>Short code: 'LAL', 'CHA'</summary>
    public string? Code { get; set; }
    public string? City { get; set; }
    public string? Conference { get; set; }
    public string? Division { get; set; }
    public string? LogoUrl { get; set; }

    public ICollection<Competition> HomeCompetitions { get; set; } = new List<Competition>();
    public ICollection<Competition> AwayCompetitions { get; set; } = new List<Competition>();
    public ICollection<CompetitionRoster> CompetitionRosters { get; set; } = new List<CompetitionRoster>();
    public ICollection<PlayerTeamAssignment> PlayerAssignments { get; set; } = new List<PlayerTeamAssignment>();
}
