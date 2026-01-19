using MyOnion.WebApi.Caching.Options;

namespace MyOnion.WebApi.Caching.Services;

public sealed class CacheInvalidationService : ICacheInvalidationService
{
    private readonly ICacheProvider _cacheProvider;
    private readonly ICacheKeyIndex _cacheKeyIndex;
    private readonly IOptionsMonitor<CachingOptions> _optionsMonitor;

    public CacheInvalidationService(
        ICacheProvider cacheProvider,
        ICacheKeyIndex cacheKeyIndex,
        IOptionsMonitor<CachingOptions> optionsMonitor)
    {
        _cacheProvider = cacheProvider;
        _cacheKeyIndex = cacheKeyIndex;
        _optionsMonitor = optionsMonitor;
    }

    public Task InvalidateKeyAsync(string key, CancellationToken ct = default)
        => InvalidateByKeyAsync(key, ct);

    public Task InvalidatePrefixAsync(string prefix, CancellationToken ct = default)
        => _cacheProvider.RemoveByPrefixAsync(prefix, ct);

    public Task InvalidateAllAsync(CancellationToken ct = default)
        => _cacheProvider.RemoveByPrefixAsync(string.Empty, ct);

    private async Task InvalidateByKeyAsync(string key, CancellationToken ct)
    {
        var options = _optionsMonitor.CurrentValue;
        if (string.Equals(options.Diagnostics?.KeyDisplayMode, CacheKeyDisplayModes.Hash, StringComparison.OrdinalIgnoreCase))
        {
            var resolved = await _cacheKeyIndex.TryResolveAsync(key, ct).ConfigureAwait(false);
            if (!string.IsNullOrWhiteSpace(resolved))
            {
                await _cacheProvider.RemoveAsync(resolved, ct).ConfigureAwait(false);
                await _cacheKeyIndex.RemoveAsync(key, ct).ConfigureAwait(false);
                return;
            }
        }

        await _cacheProvider.RemoveAsync(key, ct).ConfigureAwait(false);
    }
}
