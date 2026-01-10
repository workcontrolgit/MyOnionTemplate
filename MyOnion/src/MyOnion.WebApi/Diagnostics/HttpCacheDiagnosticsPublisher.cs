#nullable enable
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using MyOnion.Application.Interfaces.Caching;
using MyOnion.Infrastructure.Caching.Options;

namespace MyOnion.WebApi.Diagnostics;

public sealed class HttpCacheDiagnosticsPublisher : ICacheDiagnosticsPublisher
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IOptionsMonitor<CachingOptions> _optionsMonitor;

    public HttpCacheDiagnosticsPublisher(
        IHttpContextAccessor httpContextAccessor,
        IOptionsMonitor<CachingOptions> optionsMonitor)
    {
        _httpContextAccessor = httpContextAccessor;
        _optionsMonitor = optionsMonitor;
    }

    public void ReportHit() => WriteStatus("HIT");

    public void ReportMiss() => WriteStatus("MISS");

    private void WriteStatus(string status)
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
    }
}
