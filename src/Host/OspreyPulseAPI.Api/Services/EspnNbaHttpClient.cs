using System.Text.Json;

namespace OspreyPulseAPI.Api.Services;

public class EspnNbaHttpClient : IEspnNbaClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<EspnNbaHttpClient> _logger;

    public EspnNbaHttpClient(HttpClient httpClient, ILogger<EspnNbaHttpClient> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<JsonDocument> GetTeamsAsync(CancellationToken cancellationToken = default)
        => await GetJsonAsync("teams", cancellationToken);

    public async Task<JsonDocument> GetScoreboardAsync(
        DateOnly from,
        DateOnly to,
        CancellationToken cancellationToken = default)
    {
        var query = $"{from:yyyyMMdd}-{to:yyyyMMdd}";
        var path = $"scoreboard?dates={query}";
        return await GetJsonAsync(path, cancellationToken);
    }

    public async Task<JsonDocument> GetEventSummaryAsync(
        string eventId,
        CancellationToken cancellationToken = default)
    {
        var path = $"summary?event={eventId}";
        return await GetJsonAsync(path, cancellationToken);
    }

    public async Task<JsonDocument> GetTeamRosterAsync(
        string teamId,
        CancellationToken cancellationToken = default)
    {
        var path = $"teams/{teamId}/roster";
        return await GetJsonAsync(path, cancellationToken);
    }

    public async Task<JsonDocument> GetNewsAsync(CancellationToken cancellationToken = default)
        => await GetJsonAsync("news", cancellationToken);

    private async Task<JsonDocument> GetJsonAsync(
        string relativePath,
        CancellationToken cancellationToken)
    {
        try
        {
            using var response = await _httpClient.GetAsync(relativePath, cancellationToken);
            response.EnsureSuccessStatusCode();

            await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
            var document = await JsonDocument.ParseAsync(stream, cancellationToken: cancellationToken);
            return document;
        }
        catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException or JsonException)
        {
            _logger.LogError(ex, "Failed to GET ESPN NBA endpoint '{Path}'", relativePath);
            throw;
        }
    }
}

