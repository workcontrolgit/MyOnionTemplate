#nullable enable
using Microsoft.AspNetCore.Authorization;
using MyOnion.Application.Features.Dashboard.Queries.GetDashboardMetrics;

namespace MyOnion.WebApi.Controllers.v1;

[ApiVersion("1.0")]
[AllowAnonymous]
[Route("api/v{version:apiVersion}/dashboard")]
public sealed class DashboardController : BaseApiController
{
    [HttpGet("metrics")]
    [ProducesResponseType(typeof(Result<DashboardMetricsDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetMetrics()
        => Ok(await Mediator.Send(new GetDashboardMetricsQuery()));
}
