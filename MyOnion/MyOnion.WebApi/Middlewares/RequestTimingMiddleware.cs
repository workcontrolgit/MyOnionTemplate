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

        public RequestTimingMiddleware(
            RequestDelegate next,
            IOptionsMonitor<ExecutionTimingOptions> optionsMonitor,
            ILogger<RequestTimingMiddleware> logger)
        {
            _next = next ?? throw new ArgumentNullException(nameof(next));
            _optionsMonitor = optionsMonitor;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var options = _optionsMonitor.CurrentValue;
            if (!options.Enabled)
            {
                await _next(context);
                return;
            }

            var stopwatch = Stopwatch.StartNew();
            context.Items[ExecutionTimingConstants.StopwatchItemKey] = stopwatch;

            if (options.IncludeHeader)
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

                if (options.LogTimings)
                {
                    _logger.LogInformation("Request {Method} {Path} executed in {Elapsed} ms", context.Request.Method, context.Request.Path, elapsed);
                }
            }
        }
    }
}
