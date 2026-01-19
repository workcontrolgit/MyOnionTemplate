namespace MyOnion.Application.Interfaces.Caching;

public sealed record CacheEntryOptions(TimeSpan AbsoluteTtl, TimeSpan? SlidingTtl = null);
