namespace OspreyPulseAPI.Modules.Competitions.Domain;

/// <summary>
/// Competition/game status. Maps to competitions.status (SMALLINT).
/// </summary>
public enum CompetitionStatus : short
{
    Scheduled = 0,
    Live = 1,
    Finished = 2,
    Delayed = 3
}
