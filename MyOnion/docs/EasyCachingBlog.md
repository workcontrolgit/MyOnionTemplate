# EasyCaching in Template OnionAPI v10.1.2: Fast, Observable, and Invalidation-Friendly

Template OnionAPI moves caching to EasyCaching so response caches are faster, cheaper to operate, and easier to invalidate across memory or Redis. This blog highlights how the EasyCaching adapter works, how invalidation stays reliable, and how diagnostics keep cache behavior visible during development.

EasyCaching is a NuGet package that provides a unified, provider-agnostic caching abstraction for .NET, with first-party providers for in-memory, Redis, and more. Its features include async APIs, serialization helpers, and easy provider switching; the benefits are faster responses, lower database load, and consistent cache behavior across environments.


## What the EasyCaching Upgrade Does

1. **Provider Flexibility** - EasyCaching can run in-memory for local dev and Redis for distributed environments with the same API surface.
2. **Thin Adapter Layer** - `ICacheProvider` keeps application code stable while routing calls through EasyCaching providers.
3. **Cache Diagnostics** - every cache hit/miss can emit headers like `X-Cache-Status`, `X-Cache-Key`, and `X-Cache-Duration-Ms`.
4. **Invalidation Support** - keys or prefixes can be cleared via the admin endpoint or on domain events, keeping stale data under control.

## Why EasyCaching Fits the Template

- **Performance** - async cache calls short-circuit expensive queries.
- **Cost control** - fewer repeated DB queries reduce cloud spend.
- **Observability** - diagnostics headers show cache hits and key usage without extra tooling.
- **Open source** - EasyCaching stays compatible with the template's OSS-first approach.

## Example Code

The adapter is intentionally small: it formats keys, selects a provider (memory or redis), and delegates to EasyCaching.

```csharp
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
```
Source: https://github.com/workcontrolgit/MyOnionTemplate/blob/master/MyOnion/src/MyOnion.WebApi/Caching/Services/EasyCachingProviderAdapter.cs

Invalidation can clear a single key or whole prefixes, while honoring the hash-based diagnostics mode.

```csharp
private async Task InvalidateByKeyAsync(string key, CancellationToken ct)
{
    var options = _optionsMonitor.CurrentValue;
    if (string.Equals(options.Diagnostics?.KeyDisplayMode, CacheKeyDisplayModes.Hash, StringComparison.OrdinalIgnoreCase))
    {
        var resolved = await _cacheKeyIndex.TryResolveAsync(key, ct).ConfigureAwait(false);
        if (!string.IsNullOrWhiteSpace(resolved))
        {
            await _cacheProvider.RemoveAsync(resolved, ct).ConfigureAwait(false);
            await _cacheKeyIndex.RemoveAsync(key, ct).ConfigureAwait(false);
            return;
        }
    }

    await _cacheProvider.RemoveAsync(key, ct).ConfigureAwait(false);
}
```
Source: https://github.com/workcontrolgit/MyOnionTemplate/blob/master/MyOnion/src/MyOnion.WebApi/Caching/Services/CacheInvalidationService.cs

DI wiring keeps the adapter and EasyCaching providers configured from `appsettings.json`.

```csharp
services.AddEasyCaching(options =>
{
    options.WithSystemTextJson();
    options.UseInMemory(config => { }, CacheProviderNames.Memory);

    var connectionString = providerSettings.Distributed.ConnectionString;
    var provider = section.GetValue<string>(nameof(CachingOptions.Provider)) ?? CacheProviders.Memory;
    if (string.Equals(provider, CacheProviders.Distributed, StringComparison.OrdinalIgnoreCase)
        && !string.IsNullOrWhiteSpace(connectionString))
    {
        options.UseRedis(config =>
        {
            var normalized = connectionString.Contains("abortConnect", StringComparison.OrdinalIgnoreCase)
                || connectionString.Contains("AbortOnConnectFail", StringComparison.OrdinalIgnoreCase)
                ? connectionString
                : string.Concat(connectionString, ",abortConnect=false");
            config.DBConfig.Configuration = normalized;
        }, CacheProviderNames.Redis);
    }
});
```
Source: https://github.com/workcontrolgit/MyOnionTemplate/blob/master/MyOnion/src/MyOnion.WebApi/Caching/ServiceCollectionExtensions.cs

## Invalidation Use Case: Dashboard Metrics

Dashboard counts are cached for fast page loads, but they must refresh after writes. Template OnionAPI raises domain events (employee/position/department/salary updates), and the cache invalidation handler clears both the list caches and the dashboard metrics key. That keeps `Dashboard:Metrics` hot for reads and accurate after writes.

```csharp
public async Task HandleAsync(EmployeeChangedEvent domainEvent, CancellationToken ct = default)
{
    await _cacheInvalidationService.InvalidatePrefixAsync(CacheKeyPrefixes.EmployeesAll, ct);
    await _cacheInvalidationService.InvalidateKeyAsync(CacheKeyPrefixes.DashboardMetrics, ct);
}
```
Source: https://github.com/workcontrolgit/MyOnionTemplate/blob/master/MyOnion/src/MyOnion.Application/Events/Handlers/CacheInvalidationEventHandler.cs

How to verify:
- Hit `GET /api/v1/Dashboard/Metrics` twice; expect MISS then HIT via `X-Cache-Status`.
- Create or update an employee/position/department/salary range.
- Re-hit `GET /api/v1/Dashboard/Metrics`; expect MISS and refreshed counts.

## Blog Summary

- EasyCaching provides a single caching API that works for in-memory and Redis deployments.
- The adapter layer preserves existing handler behavior while enabling richer invalidation options.
- Diagnostics headers make cache behavior observable for developers and QA.
