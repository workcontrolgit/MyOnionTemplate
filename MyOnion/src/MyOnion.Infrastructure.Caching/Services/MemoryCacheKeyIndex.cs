#nullable enable
using Microsoft.Extensions.Caching.Memory;
using MyOnion.Infrastructure.Caching.Options;

namespace MyOnion.Infrastructure.Caching.Services;

public sealed class MemoryCacheKeyIndex : ICacheKeyIndex
{
    private readonly IMemoryCache _cache;
    private readonly ICacheKeyHasher _hasher;
    private readonly IOptionsMonitor<CachingOptions> _optionsMonitor;

    public MemoryCacheKeyIndex(
        IMemoryCache cache,
        ICacheKeyHasher hasher,
        IOptionsMonitor<CachingOptions> optionsMonitor)
    {
        _cache = cache;
        _hasher = hasher;
        _optionsMonitor = optionsMonitor;
    }

    public Task TrackAsync(string logicalKey, CacheEntryOptions entryOptions, CancellationToken ct = default)
    {
        var hashed = _hasher.Hash(logicalKey);
        if (string.IsNullOrWhiteSpace(hashed))
        {
            return Task.CompletedTask;
        }

        var options = _optionsMonitor.CurrentValue;
        var ttl = ResolveIndexTtlSeconds(options, entryOptions);
        var cacheKey = CacheKeyFormatter.BuildCacheKey(options, BuildHashKey(hashed));
        var cacheOptions = new MemoryCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(ttl)
        };
        if (options.ProviderSettings.Memory.SizeLimitMB is int sizeLimit && sizeLimit > 0)
        {
            cacheOptions.Size = 1;
        }

        _cache.Set(cacheKey, logicalKey, cacheOptions);
        return Task.CompletedTask;
    }

    public Task<string?> TryResolveAsync(string hashedKey, CancellationToken ct = default)
    {
        var options = _optionsMonitor.CurrentValue;
        var cacheKey = CacheKeyFormatter.BuildCacheKey(options, BuildHashKey(hashedKey));
        return Task.FromResult(_cache.TryGetValue(cacheKey, out string value) ? value : null);
    }

    public Task RemoveAsync(string hashedKey, CancellationToken ct = default)
    {
        var options = _optionsMonitor.CurrentValue;
        var cacheKey = CacheKeyFormatter.BuildCacheKey(options, BuildHashKey(hashedKey));
        _cache.Remove(cacheKey);
        return Task.CompletedTask;
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
