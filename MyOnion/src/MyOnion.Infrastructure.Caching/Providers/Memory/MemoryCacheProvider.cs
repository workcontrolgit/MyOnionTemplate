using System.Collections.Concurrent;
#nullable enable
using System.Linq;
using MyOnion.Infrastructure.Caching.Options;
using MyOnion.Infrastructure.Caching.Services;

namespace MyOnion.Infrastructure.Caching.Providers.Memory;

public sealed class MemoryCacheProvider : ICacheProvider
{
    private readonly IMemoryCache _cache;
    private readonly IOptionsMonitor<CachingOptions> _optionsMonitor;
    private readonly ICacheBypassContext _bypassContext;
    private readonly ConcurrentDictionary<string, ConcurrentDictionary<string, byte>> _prefixIndex = new(StringComparer.OrdinalIgnoreCase);
    private readonly ConcurrentDictionary<string, string> _keyToPrefix = new(StringComparer.OrdinalIgnoreCase);

    public MemoryCacheProvider(
        IMemoryCache cache,
        IOptionsMonitor<CachingOptions> optionsMonitor,
        ICacheBypassContext bypassContext)
    {
        _cache = cache;
        _optionsMonitor = optionsMonitor;
        _bypassContext = bypassContext;
    }

    public Task<T?> GetAsync<T>(string key, CancellationToken ct = default)
    {
        var options = _optionsMonitor.CurrentValue;
        if (!IsCacheEnabled(options))
        {
            return Task.FromResult<T?>(default);
        }

        var cacheKey = CacheKeyFormatter.BuildCacheKey(options, key);
        return Task.FromResult(_cache.TryGetValue(cacheKey, out T value) ? value : default);
    }

    public Task SetAsync<T>(string key, T value, CacheEntryOptions entryOptions, CancellationToken ct = default)
    {
        var options = _optionsMonitor.CurrentValue;
        if (!IsCacheEnabled(options) || entryOptions.AbsoluteTtl <= TimeSpan.Zero)
        {
            return Task.CompletedTask;
        }

        var cacheKey = CacheKeyFormatter.BuildCacheKey(options, key);
        var memoryOptions = new MemoryCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = entryOptions.AbsoluteTtl,
            SlidingExpiration = entryOptions.SlidingTtl
        };

        _cache.Set(cacheKey, value, memoryOptions);
        TrackKey(options, key, cacheKey);
        return Task.CompletedTask;
    }

    public Task RemoveAsync(string key, CancellationToken ct = default)
    {
        var options = _optionsMonitor.CurrentValue;
        var cacheKey = CacheKeyFormatter.BuildCacheKey(options, key);
        _cache.Remove(cacheKey);
        RemoveFromIndex(cacheKey);
        return Task.CompletedTask;
    }

    public Task RemoveByPrefixAsync(string prefix, CancellationToken ct = default)
    {
        var options = _optionsMonitor.CurrentValue;
        if (string.IsNullOrWhiteSpace(prefix))
        {
            foreach (var cacheKey in _keyToPrefix.Keys.ToArray())
            {
                _cache.Remove(cacheKey);
            }

            _prefixIndex.Clear();
            _keyToPrefix.Clear();
            return Task.CompletedTask;
        }

        var prefixKey = CacheKeyFormatter.BuildPrefixKey(options, prefix);
        if (_prefixIndex.TryRemove(prefixKey, out var keys))
        {
            foreach (var entry in keys.Keys)
            {
                _cache.Remove(entry);
                _keyToPrefix.TryRemove(entry, out _);
            }
        }

        return Task.CompletedTask;
    }

    private bool IsCacheEnabled(CachingOptions options) => options.Enabled && !options.DisableCache && !_bypassContext.ShouldBypass;

    private void TrackKey(CachingOptions options, string logicalKey, string cacheKey)
    {
        var logicalPrefix = ExtractPrefix(logicalKey);
        if (string.IsNullOrWhiteSpace(logicalPrefix))
        {
            return;
        }

        var prefixKey = CacheKeyFormatter.BuildPrefixKey(options, logicalPrefix);
        var set = _prefixIndex.GetOrAdd(prefixKey, _ => new ConcurrentDictionary<string, byte>(StringComparer.OrdinalIgnoreCase));
        set[cacheKey] = 0;
        _keyToPrefix[cacheKey] = prefixKey;
    }

    private void RemoveFromIndex(string cacheKey)
    {
        if (_keyToPrefix.TryRemove(cacheKey, out var prefixKey) &&
            _prefixIndex.TryGetValue(prefixKey, out var set))
        {
            set.TryRemove(cacheKey, out _);
            if (set.IsEmpty)
            {
                _prefixIndex.TryRemove(prefixKey, out _);
            }
        }
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
