#nullable enable
namespace MyOnion.WebApi.Caching.Options;

public sealed class CachingOptions
{
    public const string SectionName = "Caching";

    public bool Enabled { get; set; }

    public bool DisableCache { get; set; }

    public int DefaultCacheDurationSeconds { get; set; } = 60;

    public string Provider { get; set; } = CacheProviders.Memory;

    public string KeyPrefix { get; set; } = "MyOnion";

    public ProviderSettings ProviderSettings { get; set; } = new();

    public Dictionary<string, EndpointCacheOptions> PerEndpoint { get; set; } =
        new(StringComparer.OrdinalIgnoreCase);

    public CacheDiagnosticsOptions Diagnostics { get; set; } = new();
}

public sealed class ProviderSettings
{
    public MemoryProviderSettings Memory { get; set; } = new();

    public DistributedProviderSettings Distributed { get; set; } = new();
}

public sealed class MemoryProviderSettings
{
    public int? SizeLimitMB { get; set; }
}

public sealed class DistributedProviderSettings
{
    public string? ConnectionString { get; set; }

    public int IndexKeyTtlSeconds { get; set; } = 600;
}

public sealed class EndpointCacheOptions
{
    public int? AbsoluteTtlSeconds { get; set; }

    public int? SlidingTtlSeconds { get; set; }
}

public sealed class CacheDiagnosticsOptions
{
    public const string DefaultHeaderName = "X-Cache-Status";
    public const string DefaultKeyHeaderName = "X-Cache-Key";
    public const string DefaultDurationHeaderName = "X-Cache-Duration-Ms";
    public const string DefaultKeyDisplayMode = CacheKeyDisplayModes.Hash;

    public bool EmitCacheStatusHeader { get; set; }

    public string HeaderName { get; set; } = DefaultHeaderName;

    public string KeyHeaderName { get; set; } = DefaultKeyHeaderName;

    public string DurationHeaderName { get; set; } = DefaultDurationHeaderName;

    public string KeyDisplayMode { get; set; } = DefaultKeyDisplayMode;
}

public static class CacheProviders
{
    public const string Memory = "Memory";
    public const string Distributed = "Distributed";
}

public static class CacheKeyDisplayModes
{
    public const string Raw = "Raw";
    public const string Hash = "Hash";
}
