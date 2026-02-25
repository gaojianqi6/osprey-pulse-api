using System.Net.Http.Headers;

namespace OspreyPulseAPI.Api.Services;

/// <summary>
/// Simple per-process rate limiter for ESPN API calls.
/// Guarantees at most 1 request/second across all callers.
/// </summary>
public class EspnRateLimitedHandler : DelegatingHandler
{
    private readonly object _lock = new();
    private DateTime _lastRequestUtc = DateTime.MinValue;
    private readonly TimeSpan _minInterval = TimeSpan.FromSeconds(1);

    public EspnRateLimitedHandler()
    {
    }

    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        DateTime delayUntilUtc;

        lock (_lock)
        {
            var now = DateTime.UtcNow;
            var nextAllowed = _lastRequestUtc + _minInterval;
            if (nextAllowed <= now)
            {
                _lastRequestUtc = now;
                delayUntilUtc = now;
            }
            else
            {
                _lastRequestUtc = nextAllowed;
                delayUntilUtc = nextAllowed;
            }
        }

        var delay = delayUntilUtc - DateTime.UtcNow;
        if (delay > TimeSpan.Zero)
        {
            try
            {
                await Task.Delay(delay, cancellationToken);
            }
            catch (TaskCanceledException)
            {
                // Ignore cancellation during delay; let base handler observe cancellation.
            }
        }

        // ESPN API requires a User-Agent in some contexts; provide a simple one.
        if (request.Headers.UserAgent.Count == 0)
        {
            request.Headers.UserAgent.Add(
                new ProductInfoHeaderValue("OspreyPulseAPI", "1.0"));
        }

        return await base.SendAsync(request, cancellationToken);
    }
}

