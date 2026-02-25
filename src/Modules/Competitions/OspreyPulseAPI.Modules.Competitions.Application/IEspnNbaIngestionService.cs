namespace OspreyPulseAPI.Modules.Competitions.Application;

public interface IEspnNbaIngestionService
{
    Task EnsureTeamsAsync(CancellationToken cancellationToken = default);

    Task EnsureUpcomingThreeDayScoreboardAsync(CancellationToken cancellationToken = default);

    Task EnsureCompetitionDetailsAsync(string eventId, CancellationToken cancellationToken = default);
}

