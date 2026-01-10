#nullable enable
using System.Globalization;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using MyOnion.Application.Interfaces.Caching;
using MyOnion.Infrastructure.Caching.Options;

namespace MyOnion.WebApi.Diagnostics;

public sealed class HttpCacheDiagnosticsPublisher : ICacheDiagnosticsPublisher
{
    private const string CacheKeyHeaderName = "X-Cache-Key";
    private const string CacheDurationHeaderName = "X-Cache-Duration-Ms";
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IOptionsMonitor<CachingOptions> _optionsMonitor;

    public HttpCacheDiagnosticsPublisher(
        IHttpContextAccessor httpContextAccessor,
        IOptionsMonitor<CachingOptions> optionsMonitor)
    {
        _httpContextAccessor = httpContextAccessor;
        _optionsMonitor = optionsMonitor;
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

        context.Response.Headers[headerName] = status;
        if (!string.IsNullOrWhiteSpace(cacheKey))
        {
            context.Response.Headers[CacheKeyHeaderName] = cacheKey;
        }

        if (cacheDuration is { } duration && duration > TimeSpan.Zero)
        {
            context.Response.Headers[CacheDurationHeaderName] = ((long)duration.TotalMilliseconds).ToString(CultureInfo.InvariantCulture);
        }
    }
}
