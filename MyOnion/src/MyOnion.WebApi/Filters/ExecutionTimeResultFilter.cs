using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Options;
using MyOnion.Application.Common.Results;
using MyOnion.WebApi.Common;
using MyOnion.WebApi.Options;

namespace MyOnion.WebApi.Filters
{
    public sealed class ExecutionTimeResultFilter : IAsyncResultFilter
    {
        private readonly IOptionsMonitor<ExecutionTimingOptions> _optionsMonitor;
        private readonly IFeatureManagerSnapshot _featureManager;

        public ExecutionTimeResultFilter(
            IOptionsMonitor<ExecutionTimingOptions> optionsMonitor,
            IFeatureManagerSnapshot featureManager)
        {
            _optionsMonitor = optionsMonitor;
            _featureManager = featureManager;
        }

        public async Task OnResultExecutionAsync(ResultExecutingContext context, ResultExecutionDelegate next)
        {
            var options = _optionsMonitor.CurrentValue;
            var enabled = await _featureManager.IsEnabledAsync("ExecutionTimingEnabled").ConfigureAwait(false);
            var includePayload = await _featureManager.IsEnabledAsync("ExecutionTimingIncludePayload").ConfigureAwait(false);
            if (!enabled || !includePayload)
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
}
