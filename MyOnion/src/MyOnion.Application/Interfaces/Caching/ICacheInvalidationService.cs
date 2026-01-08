namespace MyOnion.Application.Interfaces.Caching;

public interface ICacheInvalidationService
{
    Task InvalidateKeyAsync(string key, CancellationToken ct = default);
    Task InvalidatePrefixAsync(string prefix, CancellationToken ct = default);
    Task InvalidateAllAsync(CancellationToken ct = default);
}
