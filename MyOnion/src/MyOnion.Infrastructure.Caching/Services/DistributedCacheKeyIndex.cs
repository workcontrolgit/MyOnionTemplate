#nullable enable
using System.Text;
using MyOnion.Infrastructure.Caching.Options;

namespace MyOnion.Infrastructure.Caching.Services;

public sealed class DistributedCacheKeyIndex : ICacheKeyIndex
{
    private readonly IDistributedCache _cache;
    private readonly ICacheKeyHasher _hasher;
    private readonly IOptionsMonitor<CachingOptions> _optionsMonitor;

    public DistributedCacheKeyIndex(
        IDistributedCache cache,
        ICacheKeyHasher hasher,
        IOptionsMonitor<CachingOptions> optionsMonitor)
    {
        _cache = cache;
        _hasher = hasher;
        _optionsMonitor = optionsMonitor;
    }

    public async Task TrackAsync(string logicalKey, CacheEntryOptions entryOptions, CancellationToken ct = default)
    {
        var hashed = _hasher.Hash(logicalKey);
        if (string.IsNullOrWhiteSpace(hashed))
        {
            return;
        }

        var options = _optionsMonitor.CurrentValue;
        var ttlSeconds = ResolveIndexTtlSeconds(options, entryOptions);
        var cacheKey = CacheKeyFormatter.BuildCacheKey(options, BuildHashKey(hashed));
        var payload = Encoding.UTF8.GetBytes(logicalKey);
        var distributedOptions = new DistributedCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(ttlSeconds)
        };
        await _cache.SetAsync(cacheKey, payload, distributedOptions, ct).ConfigureAwait(false);
    }

    public async Task<string?> TryResolveAsync(string hashedKey, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(hashedKey))
        {
            return null;
        }

        var options = _optionsMonitor.CurrentValue;
        var cacheKey = CacheKeyFormatter.BuildCacheKey(options, BuildHashKey(hashedKey));
        var payload = await _cache.GetAsync(cacheKey, ct).ConfigureAwait(false);
        return payload is null ? null : Encoding.UTF8.GetString(payload);
    }

    public Task RemoveAsync(string hashedKey, CancellationToken ct = default)
    {
        var options = _optionsMonitor.CurrentValue;
        var cacheKey = CacheKeyFormatter.BuildCacheKey(options, BuildHashKey(hashedKey));
        return _cache.RemoveAsync(cacheKey, ct);
    }

    private static string BuildHashKey(string hashed) => string.Concat("__hash:", hashed);

    private static int ResolveIndexTtlSeconds(CachingOptions options, CacheEntryOptions entryOptions)
    {
        var entrySeconds = (int)Math.Ceiling(entryOptions.AbsoluteTtl.TotalSeconds);
        if (entrySeconds <= 0)
        {
            entrySeconds = options.DefaultCacheDurationSeconds > 0 ? options.DefaultCacheDurationSeconds : 60;
        }

        var indexSeconds = options.ProviderSettings.Distributed.IndexKeyTtlSeconds;
        if (indexSeconds <= 0)
        {
            indexSeconds = options.DefaultCacheDurationSeconds > 0 ? options.DefaultCacheDurationSeconds : 60;
        }

        return Math.Max(entrySeconds, indexSeconds);
    }
}
