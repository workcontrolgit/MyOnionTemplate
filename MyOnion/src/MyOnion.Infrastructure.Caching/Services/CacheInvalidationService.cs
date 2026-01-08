namespace MyOnion.Infrastructure.Caching.Services;

public sealed class CacheInvalidationService : ICacheInvalidationService
{
    private readonly ICacheProvider _cacheProvider;

    public CacheInvalidationService(ICacheProvider cacheProvider)
    {
        _cacheProvider = cacheProvider;
    }

    public Task InvalidateKeyAsync(string key, CancellationToken ct = default)
        => _cacheProvider.RemoveAsync(key, ct);

    public Task InvalidatePrefixAsync(string prefix, CancellationToken ct = default)
        => _cacheProvider.RemoveByPrefixAsync(prefix, ct);

    public Task InvalidateAllAsync(CancellationToken ct = default)
        => _cacheProvider.RemoveByPrefixAsync(string.Empty, ct);
}
