namespace OspreyPulseAPI.Modules.Competitions.Domain;

/// <summary>
/// Which side in the competition. Maps to competition_rosters.team_side (SMALLINT).
/// </summary>
public enum TeamSide : short
{
    Home = 1,
    Away = 2
}
