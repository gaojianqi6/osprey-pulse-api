namespace OspreyPulseAPI.Modules.Competitions.Domain;

/// <summary>
/// Lineup status for a roster entry. Maps to competition_rosters.lineup_status (SMALLINT).
/// </summary>
public enum LineupStatus : short
{
    Starter = 1,
    Bench = 2,
    SubstitutedIn = 3
}
