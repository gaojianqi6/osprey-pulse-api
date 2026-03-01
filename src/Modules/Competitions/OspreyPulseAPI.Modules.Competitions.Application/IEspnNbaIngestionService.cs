namespace OspreyPulseAPI.Modules.Competitions.Application;

public interface IEspnNbaIngestionService
{
    Task EnsureTeamsAsync(CancellationToken cancellationToken = default);

    Task EnsureUpcomingThreeDayScoreboardAsync(CancellationToken cancellationToken = default);

    Task EnsureCompetitionDetailsAsync(string eventId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Ensures we have today's NBA news (up to 6 articles from ESPN). Idempotent per day (NY time).
    /// </summary>
    Task EnsureNbaNewsForTodayAsync(CancellationToken cancellationToken = default);
}

