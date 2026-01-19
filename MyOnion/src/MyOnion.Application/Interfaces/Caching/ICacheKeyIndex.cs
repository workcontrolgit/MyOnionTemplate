namespace MyOnion.Application.Interfaces.Caching;

public interface ICacheKeyIndex
{
    Task TrackAsync(string logicalKey, CacheEntryOptions entryOptions, CancellationToken ct = default);
    Task<string?> TryResolveAsync(string hashedKey, CancellationToken ct = default);
    Task RemoveAsync(string hashedKey, CancellationToken ct = default);
}
