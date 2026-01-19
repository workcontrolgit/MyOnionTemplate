#nullable enable
using System.Globalization;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using MyOnion.Application.Interfaces.Caching;
using MyOnion.Infrastructure.Caching.Options;

namespace MyOnion.WebApi.Diagnostics;

public sealed class HttpCacheDiagnosticsPublisher : ICacheDiagnosticsPublisher
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IOptionsMonitor<CachingOptions> _optionsMonitor;
    private readonly ICacheKeyHasher _cacheKeyHasher;

    public HttpCacheDiagnosticsPublisher(
        IHttpContextAccessor httpContextAccessor,
        IOptionsMonitor<CachingOptions> optionsMonitor,
        ICacheKeyHasher cacheKeyHasher)
    {
        _httpContextAccessor = httpContextAccessor;
        _optionsMonitor = optionsMonitor;
        _cacheKeyHasher = cacheKeyHasher;
    }

    public void ReportHit(string cacheKey, TimeSpan? cacheDuration) => WriteStatus("HIT", cacheKey, cacheDuration);

    public void ReportMiss(string cacheKey, TimeSpan? cacheDuration) => WriteStatus("MISS", cacheKey, cacheDuration);

    private void WriteStatus(string status, string cacheKey, TimeSpan? cacheDuration)
    {
        var diagnostics = _optionsMonitor.CurrentValue.Diagnostics;
        if (diagnostics is null || !diagnostics.EmitCacheStatusHeader)
        {
            return;
        }

        var context = _httpContextAccessor.HttpContext;
        if (context is null)
        {
            return;
        }

        var headerName = string.IsNullOrWhiteSpace(diagnostics.HeaderName)
            ? CacheDiagnosticsOptions.DefaultHeaderName
            : diagnostics.HeaderName;
        var keyHeaderName = string.IsNullOrWhiteSpace(diagnostics.KeyHeaderName)
            ? CacheDiagnosticsOptions.DefaultKeyHeaderName
            : diagnostics.KeyHeaderName;
        var durationHeaderName = string.IsNullOrWhiteSpace(diagnostics.DurationHeaderName)
            ? CacheDiagnosticsOptions.DefaultDurationHeaderName
            : diagnostics.DurationHeaderName;

        context.Response.Headers[headerName] = status;
        if (!string.IsNullOrWhiteSpace(cacheKey))
        {
            var keyValue = ShouldEmitRawKey(diagnostics)
                ? cacheKey
                : _cacheKeyHasher.Hash(cacheKey);
            context.Response.Headers[keyHeaderName] = keyValue;
        }

        if (cacheDuration is { } duration && duration > TimeSpan.Zero)
        {
            context.Response.Headers[durationHeaderName] = ((long)duration.TotalMilliseconds).ToString(CultureInfo.InvariantCulture);
        }
    }

    private static bool ShouldEmitRawKey(CacheDiagnosticsOptions diagnostics)
    {
        if (diagnostics is null)
        {
            return false;
        }

        return string.Equals(diagnostics.KeyDisplayMode, CacheKeyDisplayModes.Raw, StringComparison.OrdinalIgnoreCase);
    }
}
