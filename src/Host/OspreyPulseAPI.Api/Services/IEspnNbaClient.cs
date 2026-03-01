using System.Text.Json;

namespace OspreyPulseAPI.Api.Services;

public interface IEspnNbaClient
{
    Task<JsonDocument> GetTeamsAsync(CancellationToken cancellationToken = default);

    Task<JsonDocument> GetScoreboardAsync(
        DateOnly from,
        DateOnly to,
        CancellationToken cancellationToken = default);

    Task<JsonDocument> GetEventSummaryAsync(
        string eventId,
        CancellationToken cancellationToken = default);

    Task<JsonDocument> GetTeamRosterAsync(
        string teamId,
        CancellationToken cancellationToken = default);

    /// <summary>Fetches NBA news from ESPN (e.g. /news).</summary>
    Task<JsonDocument> GetNewsAsync(CancellationToken cancellationToken = default);
}

