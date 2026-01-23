#nullable enable
using EasyCaching.Core;
using MyOnion.WebApi.Caching.Options;

namespace MyOnion.WebApi.Caching.Services;

public sealed class EasyCachingProviderAdapter : ICacheProvider
{
    private readonly IEasyCachingProviderFactory _providerFactory;
    private readonly IOptionsMonitor<CachingOptions> _optionsMonitor;
    private readonly ICacheBypassContext _bypassContext;
    private readonly ICacheKeyIndex _cacheKeyIndex;

    public EasyCachingProviderAdapter(
        IEasyCachingProviderFactory providerFactory,
        IOptionsMonitor<CachingOptions> optionsMonitor,
        ICacheBypassContext bypassContext,
        ICacheKeyIndex cacheKeyIndex)
    {
        _providerFactory = providerFactory;
        _optionsMonitor = optionsMonitor;
        _bypassContext = bypassContext;
        _cacheKeyIndex = cacheKeyIndex;
    }

    public async Task<T?> GetAsync<T>(string key, CancellationToken ct = default)
    {
        var options = _optionsMonitor.CurrentValue;
        if (!IsCacheEnabled(options))
        {
            return default;
        }

        var cacheKey = CacheKeyFormatter.BuildCacheKey(options, key);
        var result = await GetProvider(options).GetAsync<T>(cacheKey, ct).ConfigureAwait(false);
        return result.HasValue ? result.Value : default;
    }

    public async Task SetAsync<T>(string key, T value, CacheEntryOptions entryOptions, CancellationToken ct = default)
    {
        var options = _optionsMonitor.CurrentValue;
        if (!IsCacheEnabled(options) || entryOptions.AbsoluteTtl <= TimeSpan.Zero)
        {
            return;
        }

        var cacheKey = CacheKeyFormatter.BuildCacheKey(options, key);
        await GetProvider(options).SetAsync(cacheKey, value, entryOptions.AbsoluteTtl, ct).ConfigureAwait(false);
        await _cacheKeyIndex.TrackAsync(key, entryOptions, ct).ConfigureAwait(false);
    }

    public Task RemoveAsync(string key, CancellationToken ct = default)
    {
        var options = _optionsMonitor.CurrentValue;
        var cacheKey = CacheKeyFormatter.BuildCacheKey(options, key);
        return GetProvider(options).RemoveAsync(cacheKey, ct);
    }

    public Task RemoveByPrefixAsync(string prefix, CancellationToken ct = default)
    {
        var options = _optionsMonitor.CurrentValue;
        if (string.IsNullOrWhiteSpace(prefix))
        {
            return GetProvider(options).FlushAsync(ct);
        }

        var prefixKey = CacheKeyFormatter.BuildPrefixKey(options, prefix);
        return GetProvider(options).RemoveByPrefixAsync(prefixKey, ct);
    }

    private bool IsCacheEnabled(CachingOptions options)
        => options.Enabled && !options.DisableCache && !_bypassContext.ShouldBypass;

    private IEasyCachingProvider GetProvider(CachingOptions options)
    {
        var providerName = string.Equals(options.Provider, CacheProviders.Distributed, StringComparison.OrdinalIgnoreCase)
            && !string.IsNullOrWhiteSpace(options.ProviderSettings.Distributed.ConnectionString)
            ? CacheProviderNames.Redis
            : CacheProviderNames.Memory;
        return _providerFactory.GetCachingProvider(providerName);
    }

    private static class CacheProviderNames
    {
        public const string Memory = "mem";
        public const string Redis = "redis";
    }
}
