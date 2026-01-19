#nullable enable
using EasyCaching.Core;
using MyOnion.WebApi.Caching.Options;

namespace MyOnion.WebApi.Caching.Services;

public sealed class EasyCachingKeyIndex : ICacheKeyIndex
{
    private readonly IEasyCachingProviderFactory _providerFactory;
    private readonly ICacheKeyHasher _hasher;
    private readonly IOptionsMonitor<CachingOptions> _optionsMonitor;

    public EasyCachingKeyIndex(
        IEasyCachingProviderFactory providerFactory,
        ICacheKeyHasher hasher,
        IOptionsMonitor<CachingOptions> optionsMonitor)
    {
        _providerFactory = providerFactory;
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
        await GetProvider(options).SetAsync(cacheKey, logicalKey, TimeSpan.FromSeconds(ttlSeconds), ct).ConfigureAwait(false);
    }

    public async Task<string?> TryResolveAsync(string hashedKey, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(hashedKey))
        {
            return null;
        }

        var options = _optionsMonitor.CurrentValue;
        var cacheKey = CacheKeyFormatter.BuildCacheKey(options, BuildHashKey(hashedKey));
        var result = await GetProvider(options).GetAsync<string>(cacheKey, ct).ConfigureAwait(false);
        return result.HasValue ? result.Value : null;
    }

    public Task RemoveAsync(string hashedKey, CancellationToken ct = default)
    {
        var options = _optionsMonitor.CurrentValue;
        var cacheKey = CacheKeyFormatter.BuildCacheKey(options, BuildHashKey(hashedKey));
        return GetProvider(options).RemoveAsync(cacheKey, ct);
    }

    private IEasyCachingProvider GetProvider(CachingOptions options)
    {
        var providerName = string.Equals(options.Provider, CacheProviders.Distributed, StringComparison.OrdinalIgnoreCase)
            && !string.IsNullOrWhiteSpace(options.ProviderSettings.Distributed.ConnectionString)
            ? CacheProviderNames.Redis
            : CacheProviderNames.Memory;
        return _providerFactory.GetCachingProvider(providerName);
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

    private static class CacheProviderNames
    {
        public const string Memory = "mem";
        public const string Redis = "redis";
    }
}
