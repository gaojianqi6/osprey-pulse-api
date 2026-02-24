namespace OspreyPulseAPI.Modules.Competitions.Domain;

/// <summary>
/// Competition/game status. Maps to API-Sports and competitions.status (SMALLINT).
/// </summary>
public enum CompetitionStatus : short
{
    NotStarted = 1,
    Live = 2,
    Finished = 3,
    Postponed = 4,
    Delayed = 5,
    Canceled = 6
}
