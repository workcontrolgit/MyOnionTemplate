using MyOnion.WebApi.Caching.Options;

namespace MyOnion.WebApi.Caching.Services;

public sealed class CacheEntryOptionsFactory : ICacheEntryOptionsFactory
{
    private readonly IOptionsMonitor<CachingOptions> _optionsMonitor;

    public CacheEntryOptionsFactory(IOptionsMonitor<CachingOptions> optionsMonitor)
    {
        _optionsMonitor = optionsMonitor;
    }

    public CacheEntryOptions Create(string endpointKey)
    {
        var options = _optionsMonitor.CurrentValue;
        if (!options.Enabled)
        {
            return new CacheEntryOptions(TimeSpan.Zero);
        }

        var lookupKey = string.IsNullOrWhiteSpace(endpointKey) ? string.Empty : endpointKey;
        options.PerEndpoint.TryGetValue(lookupKey, out var endpointOptions);

        var absoluteSeconds = endpointOptions?.AbsoluteTtlSeconds ?? options.DefaultCacheDurationSeconds;
        if (absoluteSeconds <= 0)
        {
            absoluteSeconds = options.DefaultCacheDurationSeconds > 0 ? options.DefaultCacheDurationSeconds : 60;
        }

        TimeSpan? sliding = endpointOptions?.SlidingTtlSeconds is { } slidingSeconds and > 0
            ? TimeSpan.FromSeconds(slidingSeconds)
            : null;

        return new CacheEntryOptions(TimeSpan.FromSeconds(absoluteSeconds), sliding);
    }
}
