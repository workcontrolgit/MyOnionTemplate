using System.Diagnostics;
using System.Globalization;
using Microsoft.Extensions.Options;
using MyOnion.WebApi.Common;
using MyOnion.WebApi.Options;

namespace MyOnion.WebApi.Middlewares
{
    public sealed class RequestTimingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IOptionsMonitor<ExecutionTimingOptions> _optionsMonitor;
        private readonly ILogger<RequestTimingMiddleware> _logger;
        private readonly IFeatureManager _featureManager;

        public RequestTimingMiddleware(
            RequestDelegate next,
            IOptionsMonitor<ExecutionTimingOptions> optionsMonitor,
            ILogger<RequestTimingMiddleware> logger,
            IFeatureManager featureManager)
        {
            _next = next ?? throw new ArgumentNullException(nameof(next));
            _optionsMonitor = optionsMonitor;
            _logger = logger;
            _featureManager = featureManager;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var options = _optionsMonitor.CurrentValue;
            var enabled = await _featureManager.IsEnabledAsync("ExecutionTimingEnabled").ConfigureAwait(false);
            var includeHeader = await _featureManager.IsEnabledAsync("ExecutionTimingIncludeHeader").ConfigureAwait(false);
            var logTimings = await _featureManager.IsEnabledAsync("ExecutionTimingLogTimings").ConfigureAwait(false);
            if (!enabled)
            {
                await _next(context);
                return;
            }

            var stopwatch = Stopwatch.StartNew();
            context.Items[ExecutionTimingConstants.StopwatchItemKey] = stopwatch;

            if (includeHeader)
            {
                context.Response.OnStarting(state =>
                {
                    var (httpContext, opts, sw) = ((HttpContext Context, ExecutionTimingOptions Options, Stopwatch Stopwatch))state;
                    if (!httpContext.Response.HasStarted)
                    {
                        var elapsed = sw.Elapsed.TotalMilliseconds;
                        httpContext.Items[ExecutionTimingConstants.ElapsedItemKey] = elapsed;
                        httpContext.Response.Headers[opts.HeaderName] = elapsed.ToString("0.###", CultureInfo.InvariantCulture);
                    }

                    return Task.CompletedTask;
                }, (context, options, stopwatch));
            }

            try
            {
                await _next(context);
            }
            finally
            {
                stopwatch.Stop();
                var elapsed = stopwatch.Elapsed.TotalMilliseconds;
                context.Items[ExecutionTimingConstants.ElapsedItemKey] = elapsed;

                if (logTimings)
                {
                    _logger.LogInformation("Request {Method} {Path} executed in {Elapsed} ms", context.Request.Method, context.Request.Path, elapsed);
                }
            }
        }
    }
}
