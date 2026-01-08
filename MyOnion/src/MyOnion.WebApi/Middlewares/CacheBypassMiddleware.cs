using MyOnion.Infrastructure.Caching.Options;

namespace MyOnion.WebApi.Middlewares;

public sealed class CacheBypassMiddleware
{
    public const string HeaderName = "X-Debug-Disable-Cache";

    private readonly RequestDelegate _next;
    private readonly IOptionsMonitor<CachingOptions> _cachingOptions;
    private readonly ILogger<CacheBypassMiddleware> _logger;
    private readonly IConfiguration _configuration;

    public CacheBypassMiddleware(
        RequestDelegate next,
        IOptionsMonitor<CachingOptions> cachingOptions,
        ILogger<CacheBypassMiddleware> logger,
        IConfiguration configuration)
    {
        _next = next;
        _cachingOptions = cachingOptions;
        _logger = logger;
        _configuration = configuration;
    }

    public async Task InvokeAsync(HttpContext context, ICacheBypassContext bypassContext)
    {
        try
        {
            if (ShouldBypass(context))
            {
                bypassContext.Enable("debug header");
                _logger.LogWarning("Cache bypass activated via header for {Path} by {User}", context.Request.Path, context.User.Identity?.Name ?? "anonymous");
            }

            await _next(context);
        }
        finally
        {
            bypassContext.Reset();
        }
    }

    private bool ShouldBypass(HttpContext context)
    {
        var options = _cachingOptions.CurrentValue;
        if (!options.Enabled)
        {
            return false;
        }

        if (!context.Request.Headers.TryGetValue(HeaderName, out var values))
        {
            return false;
        }

        if (!IsTruthy(values.ToString()))
        {
            return false;
        }

        var user = context.User;
        if (user?.Identity?.IsAuthenticated != true)
        {
            return false;
        }

        var adminRole = _configuration["ApiRoles:AdminRole"];
        if (string.IsNullOrWhiteSpace(adminRole) || !user.IsInRole(adminRole))
        {
            return false;
        }

        return true;
    }

    private static bool IsTruthy(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return false;
        }

        return value.Equals("true", StringComparison.OrdinalIgnoreCase)
               || value.Equals("1")
               || value.Equals("yes", StringComparison.OrdinalIgnoreCase)
               || value.Equals("on", StringComparison.OrdinalIgnoreCase);
    }
}
