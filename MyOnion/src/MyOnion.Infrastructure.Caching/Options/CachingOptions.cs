#nullable enable
namespace MyOnion.Infrastructure.Caching.Options;

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

    public bool EmitCacheStatusHeader { get; set; }

    public string HeaderName { get; set; } = DefaultHeaderName;
}

public static class CacheProviders
{
    public const string Memory = "Memory";
    public const string Distributed = "Distributed";
}
