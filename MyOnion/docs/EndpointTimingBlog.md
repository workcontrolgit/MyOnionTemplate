# Measuring API Performance with Execution Timing in Template OnionAPI

Template OnionAPI bakes endpoint timing into the pipeline so developers can see how every request behaves. Instead of guessing how long a controller took, middleware captures the elapsed time once, adds the metric to responses, and logs it for later analysis. Here is how the feature is structured, why it matters, and how you can use it in your own APIs.

## What the Execution Timing Feature Does

The timing system has three parts:

1. **RequestTimingMiddleware** – wraps each request in a `Stopwatch`. It stores the running stopwatch in `HttpContext.Items`, writes an optional response header (for example `x-execution-time-ms`), and logs the elapsed time when the request completes.
2. **ExecutionTimeResultFilter** – runs after MVC produces a `Result` or `Result<T>`. It reads the elapsed time from `HttpContext.Items` and calls `SetExecutionTime` so every JSON payload already includes the metric.
3. **ExecutionTimingOptions** – toggles timing per environment. You can enable/disable measurement, add response headers, and control logging without recompiling.

This matches EndpointTimingPlan.md: measure once, surface everywhere, and minimize overhead.

## Why Track Endpoint Performance

- **Fast feedback** – Developers can see the execution time in every API response (both the JSON `Result` payload and headers) without attaching a profiler.
- **Client visibility** – Consumers get a reliable `executionTimeMs` field so they can correlate slow calls with UI behavior or retries.
- **Low friction observability** – Structured logs like `Request GET /api/positions executed in 18.3 ms` land in your logging provider for dashboards and alerts.
- **Configurable** – Timing is just another option in `appsettings.json`, so you can turn it off in sensitive environments or disable headers for external APIs.

## Example: RequestTimingMiddleware

```csharp
public sealed class RequestTimingMiddleware
{
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
```
Source: `MyOnion/src/MyOnion.WebApi/Middlewares/RequestTimingMiddleware.cs:1`

The middleware centralizes measurement, so controllers stay clean. It also ensures the elapsed time is stored even if the header isn’t emitted (for example, in private environments).

## Example: ExecutionTimeResultFilter

```csharp
public sealed class ExecutionTimeResultFilter : IAsyncResultFilter
{
    public async Task OnResultExecutionAsync(ResultExecutingContext context, ResultExecutionDelegate next)
    {
        var options = _optionsMonitor.CurrentValue;
        if (!options.Enabled || !options.IncludeResultPayload)
        {
            await next();
            return;
        }

        if (context.Result is ObjectResult objectResult &&
            objectResult.Value is Result baseResult)
        {
            if (context.HttpContext.Items.TryGetValue(ExecutionTimingConstants.StopwatchItemKey, out var stopwatchObj) &&
                stopwatchObj is Stopwatch stopwatch)
            {
                baseResult.SetExecutionTime(stopwatch.Elapsed.TotalMilliseconds);
            }
            else if (context.HttpContext.Items.TryGetValue(ExecutionTimingConstants.ElapsedItemKey, out var elapsedObj) &&
                elapsedObj is double elapsed)
            {
                baseResult.SetExecutionTime(elapsed);
            }
        }

        await next();
    }
}
```
Source: `MyOnion/src/MyOnion.WebApi/Filters/ExecutionTimeResultFilter.cs:1`

Because `Result`/`Result<T>` already exposes `ExecutionTimeMs`, clients automatically see the number in every payload without custom formatting.

## Getting Started

1. **Enable timing in configuration.** In `appsettings.json`, set:

```json
"ExecutionTiming": {
  "Enabled": true,
  "IncludeHeader": true,
  "HeaderName": "x-execution-time-ms",
  "IncludeResultPayload": true,
  "LogTimings": true
}
```

2. **Register the middleware early.** Ensure `RequestTimingMiddleware` runs before exception handling and MVC.
3. **Add the result filter globally.** `ExecutionTimeResultFilter` should be part of MVC filters so every `Result` object gets enriched.
4. **Monitor logs and headers.** Use the metrics to tune slow endpoints, set alerts, or feed dashboards.

With this setup, Template OnionAPI treats performance measurement as a first-class feature. Developers can debug latency issues without extra tooling, clients gain transparent execution-time data, and operations teams get structured logs for long-term trends—all from a few reusable components.
