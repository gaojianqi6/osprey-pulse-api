using OspreyPulseAPI.Modules.Competitions.Application;

namespace OspreyPulseAPI.Api.Services;

/// <summary>
/// On startup: ensures today's NBA news is loaded if missing.
/// Then runs at midnight New York time every day to refresh news (using PeriodicTimer).
/// </summary>
public class NbaNewsIngestionHostedService : IHostedService, IDisposable
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<NbaNewsIngestionHostedService> _logger;
    private Task? _runTask;
    private CancellationTokenSource? _cts;

    public NbaNewsIngestionHostedService(
        IServiceScopeFactory scopeFactory,
        ILogger<NbaNewsIngestionHostedService> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        // 1) Run once on startup: load today's news if we don't have it yet
        try
        {
            await using var scope = _scopeFactory.CreateAsyncScope();
            var ingestion = scope.ServiceProvider.GetRequiredService<IEspnNbaIngestionService>();
            await ingestion.EnsureNbaNewsForTodayAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Startup NBA news ingestion failed.");
        }

        // 2) Schedule daily run at midnight New York time (PeriodicTimer every minute, run at ~00:00 NY)
        _cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        _runTask = RunDailyAtMidnightNyAsync(_cts.Token);
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        _cts?.Cancel();
        if (_runTask != null)
            await Task.WhenAny(_runTask, Task.Delay(Timeout.Infinite, cancellationToken));
    }

    public void Dispose() => _cts?.Dispose();

    private async Task RunDailyAtMidnightNyAsync(CancellationToken cancellationToken)
    {
        var eastern = TimeZoneInfo.FindSystemTimeZoneById(
            OperatingSystem.IsWindows() ? "Eastern Standard Time" : "America/New_York");

        using var timer = new PeriodicTimer(TimeSpan.FromMinutes(1));
        DateOnly? lastRunDateNy = null;

        while (await timer.WaitForNextTickAsync(cancellationToken))
        {
            try
            {
                var nowNy = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, eastern);
                var todayNy = DateOnly.FromDateTime(nowNy);

                // Run only when we're in the first minute of the day (00:00–00:01 NY)
                if (nowNy.Hour == 0 && nowNy.Minute < 2)
                {
                    if (lastRunDateNy == null || todayNy > lastRunDateNy)
                    {
                        await using var scope = _scopeFactory.CreateAsyncScope();
                        var ingestion = scope.ServiceProvider.GetRequiredService<IEspnNbaIngestionService>();
                        await ingestion.EnsureNbaNewsForTodayAsync(cancellationToken);
                        lastRunDateNy = todayNy;
                        _logger.LogInformation("Daily NBA news run completed for {Date} (NY).", todayNy);
                    }
                }
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Daily NBA news ingestion failed.");
            }
        }
    }
}
