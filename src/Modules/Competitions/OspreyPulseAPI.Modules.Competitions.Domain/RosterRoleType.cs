namespace OspreyPulseAPI.Modules.Competitions.Domain;

/// <summary>
/// Role in a competition roster. Maps to competition_rosters.role_type (SMALLINT).
/// </summary>
public enum RosterRoleType : short
{
    Player = 1,
    Coach = 2,
    Referee = 3
}
