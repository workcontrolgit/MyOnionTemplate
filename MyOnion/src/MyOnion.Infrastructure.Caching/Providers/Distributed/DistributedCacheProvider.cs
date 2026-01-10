#nullable enable
using System.Text;
using System.Text.Json;
using MyOnion.Infrastructure.Caching.Options;
using MyOnion.Infrastructure.Caching.Services;

namespace MyOnion.Infrastructure.Caching.Providers.Distributed;

public sealed class DistributedCacheProvider : ICacheProvider
{
    private static readonly JsonSerializerOptions SerializerOptions = new(JsonSerializerDefaults.Web)
    {
        ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles
    };

    private readonly IDistributedCache _cache;
    private readonly IOptionsMonitor<CachingOptions> _optionsMonitor;
    private readonly ICacheBypassContext _bypassContext;

    public DistributedCacheProvider(
        IDistributedCache cache,
        IOptionsMonitor<CachingOptions> optionsMonitor,
        ICacheBypassContext bypassContext)
    {
        _cache = cache;
        _optionsMonitor = optionsMonitor;
        _bypassContext = bypassContext;
    }

    public async Task<T?> GetAsync<T>(string key, CancellationToken ct = default)
    {
        var options = _optionsMonitor.CurrentValue;
        if (!IsCacheEnabled(options))
        {
            return default;
        }

        var cacheKey = CacheKeyFormatter.BuildCacheKey(options, key);
        var payload = await _cache.GetAsync(cacheKey, ct).ConfigureAwait(false);
        if (payload is null)
        {
            return default;
        }

        return JsonSerializer.Deserialize<T>(payload, SerializerOptions);
    }

    public async Task SetAsync<T>(string key, T value, CacheEntryOptions entryOptions, CancellationToken ct = default)
    {
        var options = _optionsMonitor.CurrentValue;
        if (!IsCacheEnabled(options) || entryOptions.AbsoluteTtl <= TimeSpan.Zero)
        {
            return;
        }

        var cacheKey = CacheKeyFormatter.BuildCacheKey(options, key);
        var payload = JsonSerializer.SerializeToUtf8Bytes(value, SerializerOptions);
        var distributedOptions = new DistributedCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = entryOptions.AbsoluteTtl,
            SlidingExpiration = entryOptions.SlidingTtl
        };

        await _cache.SetAsync(cacheKey, payload, distributedOptions, ct).ConfigureAwait(false);
        await TrackKeyAsync(options, key, cacheKey, ct).ConfigureAwait(false);
    }

    public async Task RemoveAsync(string key, CancellationToken ct = default)
    {
        var options = _optionsMonitor.CurrentValue;
        var cacheKey = CacheKeyFormatter.BuildCacheKey(options, key);
        await _cache.RemoveAsync(cacheKey, ct).ConfigureAwait(false);
        await RemoveFromIndexAsync(options, key, cacheKey, ct).ConfigureAwait(false);
    }

    public async Task RemoveByPrefixAsync(string prefix, CancellationToken ct = default)
    {
        var options = _optionsMonitor.CurrentValue;
        if (string.IsNullOrWhiteSpace(prefix))
        {
            await RemoveAllPrefixesAsync(options, ct).ConfigureAwait(false);
            return;
        }

        var prefixKey = CacheKeyFormatter.BuildPrefixKey(options, prefix);
        await RemoveByPrefixKeyAsync(options, prefixKey, ct).ConfigureAwait(false);
    }

    private bool IsCacheEnabled(CachingOptions options) => options.Enabled && !options.DisableCache && !_bypassContext.ShouldBypass;

    private async Task TrackKeyAsync(CachingOptions options, string logicalKey, string cacheKey, CancellationToken ct)
    {
        var logicalPrefix = ExtractPrefix(logicalKey);
        if (string.IsNullOrWhiteSpace(logicalPrefix))
        {
            return;
        }

        var prefixKey = CacheKeyFormatter.BuildPrefixKey(options, logicalPrefix);
        var indexKey = BuildIndexKey(prefixKey);
        var keys = await ReadTrackedKeysAsync(indexKey, ct).ConfigureAwait(false);
        if (!keys.Contains(cacheKey))
        {
            keys.Add(cacheKey);
            await WriteTrackedKeysAsync(options, indexKey, keys, ct).ConfigureAwait(false);
        }

        await AddPrefixToCatalogAsync(options, prefixKey, ct).ConfigureAwait(false);
    }

    private async Task RemoveFromIndexAsync(CachingOptions options, string logicalKey, string cacheKey, CancellationToken ct)
    {
        var logicalPrefix = ExtractPrefix(logicalKey);
        if (string.IsNullOrWhiteSpace(logicalPrefix))
        {
            return;
        }

        var prefixKey = CacheKeyFormatter.BuildPrefixKey(options, logicalPrefix);
        var indexKey = BuildIndexKey(prefixKey);
        var keys = await ReadTrackedKeysAsync(indexKey, ct).ConfigureAwait(false);
        if (!keys.Remove(cacheKey))
        {
            return;
        }

        if (keys.Count == 0)
        {
            await _cache.RemoveAsync(indexKey, ct).ConfigureAwait(false);
            await RemovePrefixFromCatalogAsync(options, prefixKey, ct).ConfigureAwait(false);
        }
        else
        {
            await WriteTrackedKeysAsync(options, indexKey, keys, ct).ConfigureAwait(false);
        }
    }

    private async Task RemoveAllPrefixesAsync(CachingOptions options, CancellationToken ct)
    {
        var catalogKey = BuildCatalogKey(options);
        var prefixKeys = await ReadTrackedKeysAsync(catalogKey, ct).ConfigureAwait(false);
        foreach (var prefixKey in prefixKeys)
        {
            await RemoveByPrefixKeyAsync(options, prefixKey, ct).ConfigureAwait(false);
        }

        await _cache.RemoveAsync(catalogKey, ct).ConfigureAwait(false);
    }

    private async Task RemoveByPrefixKeyAsync(CachingOptions options, string prefixKey, CancellationToken ct)
    {
        var indexKey = BuildIndexKey(prefixKey);
        var trackedKeys = await ReadTrackedKeysAsync(indexKey, ct).ConfigureAwait(false);
        foreach (var cacheKey in trackedKeys)
        {
            await _cache.RemoveAsync(cacheKey, ct).ConfigureAwait(false);
        }

        await _cache.RemoveAsync(indexKey, ct).ConfigureAwait(false);
        await RemovePrefixFromCatalogAsync(options, prefixKey, ct).ConfigureAwait(false);
    }

    private async Task AddPrefixToCatalogAsync(CachingOptions options, string prefixKey, CancellationToken ct)
    {
        var catalogKey = BuildCatalogKey(options);
        var catalog = await ReadTrackedKeysAsync(catalogKey, ct).ConfigureAwait(false);
        if (catalog.Contains(prefixKey))
        {
            return;
        }

        catalog.Add(prefixKey);
        await WriteTrackedKeysAsync(options, catalogKey, catalog, ct).ConfigureAwait(false);
    }

    private async Task RemovePrefixFromCatalogAsync(CachingOptions options, string prefixKey, CancellationToken ct)
    {
        var catalogKey = BuildCatalogKey(options);
        var catalog = await ReadTrackedKeysAsync(catalogKey, ct).ConfigureAwait(false);
        if (!catalog.Remove(prefixKey))
        {
            return;
        }

        if (catalog.Count == 0)
        {
            await _cache.RemoveAsync(catalogKey, ct).ConfigureAwait(false);
        }
        else
        {
            await WriteTrackedKeysAsync(options, catalogKey, catalog, ct).ConfigureAwait(false);
        }
    }

    private static string BuildIndexKey(string prefixKey) => string.Concat(prefixKey, ":__index");

    private static string BuildCatalogKey(CachingOptions options) => string.Concat(options.KeyPrefix, ":__prefix_catalog");

    private async Task<List<string>> ReadTrackedKeysAsync(string indexKey, CancellationToken ct)
    {
        var payload = await _cache.GetAsync(indexKey, ct).ConfigureAwait(false);
        if (payload is null)
        {
            return new List<string>();
        }

        var json = Encoding.UTF8.GetString(payload);
        return JsonSerializer.Deserialize<List<string>>(json, SerializerOptions) ?? new List<string>();
    }

    private Task WriteTrackedKeysAsync(CachingOptions options, string indexKey, List<string> keys, CancellationToken ct)
    {
        var json = JsonSerializer.SerializeToUtf8Bytes(keys, SerializerOptions);
        var ttlSeconds = options.ProviderSettings.Distributed.IndexKeyTtlSeconds;
        var distributedOptions = new DistributedCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(ttlSeconds > 0 ? ttlSeconds : options.DefaultCacheDurationSeconds)
        };
        return _cache.SetAsync(indexKey, json, distributedOptions, ct);
    }

    private static string ExtractPrefix(string logicalKey)
    {
        if (string.IsNullOrWhiteSpace(logicalKey))
        {
            return string.Empty;
        }

        var searchIndex = 0;
        while (searchIndex < logicalKey.Length)
        {
            var colonIndex = logicalKey.IndexOf(':', searchIndex);
            if (colonIndex < 0)
            {
                break;
            }

            var nextColonIndex = logicalKey.IndexOf(':', colonIndex + 1);
            var equalsIndex = logicalKey.IndexOf('=', colonIndex + 1);
            if (equalsIndex > colonIndex && (nextColonIndex < 0 || equalsIndex < nextColonIndex))
            {
                return logicalKey[..colonIndex];
            }

            searchIndex = colonIndex + 1;
        }

        return logicalKey;
    }
}
