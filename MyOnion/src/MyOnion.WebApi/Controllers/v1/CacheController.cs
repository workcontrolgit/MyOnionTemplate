#nullable enable
using Microsoft.AspNetCore.Authorization;
using MyOnion.WebApi.Models;

namespace MyOnion.WebApi.Controllers.v1;

[ApiVersion("1.0")]
[Authorize(Policy = AuthorizationConsts.AdminPolicy)]
[Route("api/v{version:apiVersion}/cache")]
public sealed class CacheController : BaseApiController
{
    private readonly ICacheInvalidationService _cacheInvalidationService;

    public CacheController(ICacheInvalidationService cacheInvalidationService)
    {
        _cacheInvalidationService = cacheInvalidationService;
    }

    [HttpPost("invalidate")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> InvalidateAsync([FromBody] CacheInvalidationRequest? request, [FromQuery] bool? invalidateAll, CancellationToken cancellationToken)
    {
        var payload = request ?? new CacheInvalidationRequest();
        var shouldInvalidateAll = payload.InvalidateAll || invalidateAll.GetValueOrDefault();

        if (shouldInvalidateAll)
        {
            await _cacheInvalidationService.InvalidateAllAsync(cancellationToken);
            return NoContent();
        }

        if (!string.IsNullOrWhiteSpace(payload.Key))
        {
            await _cacheInvalidationService.InvalidateKeyAsync(payload.Key, cancellationToken);
            return NoContent();
        }

        if (!string.IsNullOrWhiteSpace(payload.Prefix))
        {
            await _cacheInvalidationService.InvalidatePrefixAsync(payload.Prefix, cancellationToken);
            return NoContent();
        }

        return BadRequest("Specify a key, prefix, or set invalidateAll=true.");
    }
}
