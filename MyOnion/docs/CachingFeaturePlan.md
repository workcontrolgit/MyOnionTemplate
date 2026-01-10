# API Caching Feature Plan

Template OnionAPI needs a first-class caching layer so frequently requested endpoints can short-circuit repository work. **Use Case – Contoso HR (Fortune 500):** the HR analytics team exports employee rosters to a payroll vendor every 15 minutes. Without caching, each export triggers expensive EF Core queries, spikes DTU usage, and forces the team to run larger SQL/Redis tiers. With caching, the first request warms `Employees:All` for five minutes so exports, internal directories, and onboarding dashboards reuse the same payload. This reduced query volume lets Contoso run smaller database tiers and delay adding extra API instances. This project documents the requirements before implementation, including configuration, providers, cache key guidelines, invalidation strategies, and sample code locations.

## Goals
- Reduce load on EF Core and downstream services for read-heavy endpoints.
- Keep caching configurable per environment with sensible defaults (disabled for development/local).
- Support both in-memory and distributed providers without duplicating code paths.
- Provide a unified way to invalidate caches when data changes or via administrative commands.
- Ensure cache keys are stable (canonical) so equivalent requests hit the same cache entry.
- Keep cached payloads compact and safe to serialize by caching DTO-style projections instead of EF graphs.

## Configuration Requirements
- Add `Caching` section to `appsettings.*.json` containing:
  - `Enabled` (bool) - default `false` for `Development`, `true` for other environments unless overridden.
  - `DefaultCacheDurationSeconds` (int) - default `60`, validates for > 0 when caching is enabled. Per-endpoint overrides reference this value when none are supplied.
  - `Provider` (enum/string: `Memory`, `Distributed`).
  - `KeyPrefix` (string) for multi-tenant separation.
- Bind settings to `CachingOptions` with `IOptionsMonitor` so runtime toggles/hot reload are possible.
- Include a global `DisableCache` flag for troubleshooting (overrides all other settings when `true`).
- Support per-request overrides via a secure `X-Debug-Disable-Cache` header. Only process this header when the caller is authenticated and authorized (e.g., Admin role) and log every use.
- Emit cache diagnostics headers for developers:
  - `X-Cache-Status` = `HIT|MISS`
  - `X-Cache-Key` = computed cache key
  - `X-Cache-Duration-Ms` = remaining TTL for hits (absolute TTL)

## Provider Support
- **Memory:** Wrap `IMemoryCache` and register it when provider is `Memory`. Ensure singleton service with size-limiting configuration to avoid unbounded growth.
- **Distributed:** Support `IDistributedCache` so Redis/SQL Server providers can plug in. Leverage JSON serialization for values.
- Provide `ICacheProvider` abstraction exposing `GetAsync`, `SetAsync`, `RemoveAsync`, and `RemoveByPrefixAsync` to keep service code independent from the provider implementation. Because `IDistributedCache` lacks native prefix removal, maintain a prefix index (e.g., `Caching:Index:{Prefix}` stored in the distributed cache) that tracks keys per prefix so `RemoveByPrefixAsync` can iterate the stored key set.
- When `IMemoryCache` uses a `SizeLimit`, every cache entry must set a non-zero `Size` or it will throw.

## Code Locations & Layering
- **Interfaces/Contracts:** add `ICacheProvider` and `ICacheInvalidationService` under `MyOnion.Application/Interfaces` so handlers reference only abstractions.
- **Implementations:** live in the new `MyOnion.Infrastructure.Caching` project (e.g., `MemoryCacheProvider`, `DistributedCacheProvider`, `CacheInvalidationService`).
- **Alternative Shared Placement:** if we need these interfaces outside the application assembly, we can move them into `MyOnion.Infrastructure.Shared` (or another shared project) while still keeping implementations inside `MyOnion.Infrastructure.Caching`.
- **Composition Root:** `MyOnion.WebApi` (in `Program.cs` or `ServiceCollectionExtensions`) binds `CachingOptions` and selects the provider at runtime, defaulting to disabled caching for development profiles.
- **Caching behavior:** implement cache logic as MediatR pipeline behaviors (not request handler decorators) to avoid recursive handler resolution. Register behaviors with Scrutor scanning for `IPipelineBehavior<,>`.

## Example Interfaces & Providers

```csharp
// MyOnion.Application/Interfaces/ICacheProvider.cs
public interface ICacheProvider
{
    Task<T?> GetAsync<T>(string key, CancellationToken ct = default);
    Task SetAsync<T>(string key, T value, CacheEntryOptions options, CancellationToken ct = default);
    Task RemoveAsync(string key, CancellationToken ct = default);
    Task RemoveByPrefixAsync(string prefix, CancellationToken ct = default);
}

public sealed record CacheEntryOptions(TimeSpan AbsoluteTtl, TimeSpan? SlidingTtl = null);
```

```csharp
// MyOnion.Infrastructure.Caching/Memory/MemoryCacheProvider.cs
public sealed class MemoryCacheProvider : ICacheProvider
{
    private readonly IMemoryCache _cache;
    private readonly CachingOptions _options;

    public MemoryCacheProvider(IMemoryCache cache, IOptions<CachingOptions> options)
    {
        _cache = cache;
        _options = options.Value;
    }

    public Task<T?> GetAsync<T>(string key, CancellationToken ct = default)
    {
        if (!_options.Enabled) return Task.FromResult<T?>(default);
        return Task.FromResult(_cache.TryGetValue(BuildKey(key), out T value) ? value : default);
    }

    public Task SetAsync<T>(string key, T value, CacheEntryOptions entryOptions, CancellationToken ct = default)
    {
        if (!_options.Enabled || _options.DisableCache) return Task.CompletedTask;

        var opts = new MemoryCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = entryOptions.AbsoluteTtl,
            SlidingExpiration = entryOptions.SlidingTtl
        };

        _cache.Set(BuildKey(key), value, opts);
        return Task.CompletedTask;
    }

    public Task RemoveAsync(string key, CancellationToken ct = default)
    {
        _cache.Remove(BuildKey(key));
        return Task.CompletedTask;
    }

    public Task RemoveByPrefixAsync(string prefix, CancellationToken ct = default)
    {
        foreach (var cacheKey in _options.Index.GetKeys(prefix))
        {
            _cache.Remove(cacheKey);
        }

        return Task.CompletedTask;
    }

    private string BuildKey(string key) => $"{_options.KeyPrefix}:{key}";
}
```

### appsettings.json Example

```jsonc
{
  "Caching": {
    "Enabled": true,
    "DisableCache": false,
    "DefaultCacheDurationSeconds": 60,
    "Provider": "Memory",
    "KeyPrefix": "MyOnion",
    "ProviderSettings": {
      "Memory": {
        "SizeLimitMB": 256
      },
      "Distributed": {
        "ConnectionString": "redis:6379",
        "IndexKeyTtlSeconds": 600
      }
    },
    "PerEndpoint": {
      "Employees:GetAll": {
        "AbsoluteTtlSeconds": 300,
        "SlidingTtlSeconds": 120
      },
      "Positions:GetAll": {
        "AbsoluteTtlSeconds": 180
      }
    }
  }
}
```

## Cache Key Strategy
- Standardize keys as `{Prefix}:{Domain}:{Identifier}` (for example, `MyOnion:Employees:ById:1234`).
- Provide helper utilities to compose keys and to record dependencies per aggregate (e.g., every position record includes a `Positions:All` marker).
- For list endpoints, include query fingerprints (sorted filter parameters + page number) to avoid collisions.
- Canonicalize list query parameters (trim, lowercase, sort fields, de-dupe) before computing keys so different field orders map to one key.

## Invalidation Options
- Expose an `ICacheInvalidationService` with methods to purge by key, prefix, or entire cache.
- Add an administrative endpoint (authorized role) that can accept `invalidateAll=true` to clear caches when needed.
- Hook repository write operations (create/update/delete) to invalidate relevant prefixes.
- When provider is `Distributed`, support optional pub/sub or `IDistributedCache.Remove` to propagate invalidations across nodes.

## Implementation Outline
1. New project `MyOnion.Infrastructure.Caching` containing options, provider implementations, and invalidation service.
2. Register the caching module in `MyOnion.WebApi` composition root, binding options and selecting provider at runtime.
3. Introduce caching pipeline behaviors for read-heavy queries (e.g., `GetEmployeesQueryHandler`) that accept `CacheEntryOptions` so handlers can opt into custom TTLs or sliding expirations; fall back to `DefaultCacheDurationSeconds`. Register behaviors via Scrutor scanning for `IPipelineBehavior<,>`.
4. Add middleware or filters to respect the secure `X-Debug-Disable-Cache` header for debugging.
5. Extend health checks to report cache provider status (memory usage, distributed connectivity).
6. Cache DTO-style projections for list endpoints to avoid serializing EF navigation graphs; skip caching when a request includes heavy nav fields.

## Rollout Steps
1. Scaffold `MyOnion.Infrastructure.Caching` project with `CachingOptions`, provider interfaces, and dependency injection extensions.
2. Update solution files and pipelines to build/test the new project.
3. Implement memory + distributed providers with shared abstractions and configuration binding.
4. Add caching decorators and sample usage around employee/position list endpoints.
5. Implement administrative invalidation endpoint + repository invalidation hooks.
6. Document configuration samples and usage in README/docs.

## Feature Benefits
- **Performance:** cached read responses reduce DB trips, freeing resources for write operations.
- **Configurability:** teams can toggle caching per environment and choose providers without code changes.
- **Flexibility:** interfaces in the application (or shared) project keep business logic decoupled from storage details.
- **Operability:** invalidation hooks, admin endpoints, and provider diagnostics simplify troubleshooting and operations.
- **Cost Efficiency:** fewer round-trips to SQL/Redis or third-party APIs mean lower consumption-based costs and the ability to scale down compute tiers.


## Risks & Mitigations
- **Configuration Drift:** enforce required settings via validation attributes + startup checks.
- **Stale Data:** rely on invalidation service + repository hooks; add monitoring to detect high cache age.
- **Distributed Provider Failures:** wrap distributed calls with fallbacks to pass-through behavior when the cache backend is unavailable.
- **Memory Pressure:** allow memory provider to set size limits and expose metrics for dashboards.
- **Cache Key Explosion:** normalize fields/filters, cap entries with size limits, and avoid caching for high-cardinality field sets.
- **Serialization Loops:** cache DTO projections instead of EF graphs; avoid navigation properties in cached payloads unless explicitly shaped.
