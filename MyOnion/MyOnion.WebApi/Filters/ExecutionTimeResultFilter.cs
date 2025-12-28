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

        public ExecutionTimeResultFilter(IOptionsMonitor<ExecutionTimingOptions> optionsMonitor)
        {
            _optionsMonitor = optionsMonitor;
        }

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
}
