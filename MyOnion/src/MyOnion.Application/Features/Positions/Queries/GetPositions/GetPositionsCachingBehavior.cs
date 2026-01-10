#nullable enable
using System.Text;
using MyOnion.Application.Common.Caching;
using MyOnion.Application.Interfaces.Caching;

namespace MyOnion.Application.Features.Positions.Queries.GetPositions;

public sealed class GetPositionsCachingBehavior : IPipelineBehavior<GetPositionsQuery, PagedResult<IEnumerable<Entity>>>
{
    private static string EndpointKey => CacheKeyPrefixes.PositionsAll;
    private readonly ICacheProvider _cacheProvider;
    private readonly ICacheEntryOptionsFactory _entryOptionsFactory;
    private readonly ICacheDiagnosticsPublisher _diagnosticsPublisher;

    public GetPositionsCachingBehavior(
        ICacheProvider cacheProvider,
        ICacheEntryOptionsFactory entryOptionsFactory,
        ICacheDiagnosticsPublisher diagnosticsPublisher)
    {
        _cacheProvider = cacheProvider;
        _entryOptionsFactory = entryOptionsFactory;
        _diagnosticsPublisher = diagnosticsPublisher;
    }

    public async Task<PagedResult<IEnumerable<Entity>>> Handle(
        GetPositionsQuery request,
        RequestHandlerDelegate<PagedResult<IEnumerable<Entity>>> next,
        CancellationToken cancellationToken)
    {
        var cacheKey = BuildCacheKey(request);
        var entryOptions = _entryOptionsFactory.Create(EndpointKey);
        var cachedResponse = await _cacheProvider.GetAsync<CachedPositionsPage>(cacheKey, cancellationToken).ConfigureAwait(false);
        if (cachedResponse is not null)
        {
            var remainingTtl = cachedResponse.ExpiresAtUtc is { } expiresAt
                ? expiresAt - DateTimeOffset.UtcNow
                : (TimeSpan?)null;
            _diagnosticsPublisher.ReportHit(cacheKey, remainingTtl);
            return ToPagedResult(cachedResponse);
        }

        var response = await next().ConfigureAwait(false);
        if (!response.IsSuccess)
        {
            return response;
        }

        _diagnosticsPublisher.ReportMiss(cacheKey, null);
        if (!TryBuildCachePayload(response, out var payload))
        {
            return response;
        }

        payload = payload with { ExpiresAtUtc = DateTimeOffset.UtcNow.Add(entryOptions.AbsoluteTtl) };
        await _cacheProvider.SetAsync(cacheKey, payload, entryOptions, cancellationToken).ConfigureAwait(false);
        return response;
    }

    private static string BuildCacheKey(GetPositionsQuery request)
    {
        var builder = new StringBuilder(EndpointKey);
        builder.Append(":page=").Append(request.PageNumber);
        builder.Append(":size=").Append(request.PageSize);
        AppendFilter(builder, "number", NormalizeValue(request.PositionNumber));
        AppendFilter(builder, "title", NormalizeValue(request.PositionTitle));
        AppendFilter(builder, "department", NormalizeValue(request.Department));
        AppendFilter(builder, "fields", NormalizeFieldsForKey(request.Fields));
        AppendFilter(builder, "order", NormalizeValue(request.OrderBy));
        return builder.ToString();
    }

    private static void AppendFilter(StringBuilder builder, string alias, string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return;
        }

        builder.Append(':').Append(alias).Append('=').Append(value.Trim().ToLowerInvariant());
    }

    private static string NormalizeValue(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? string.Empty : value.Trim().ToLowerInvariant();
    }

    private static string NormalizeFieldsForKey(string? fields)
    {
        if (string.IsNullOrWhiteSpace(fields))
        {
            return string.Empty;
        }

        var tokens = fields
            .Split(',', StringSplitOptions.RemoveEmptyEntries)
            .Select(field => field.Trim())
            .Where(field => !string.IsNullOrWhiteSpace(field))
            .Select(field => field.ToLowerInvariant())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(field => field, StringComparer.OrdinalIgnoreCase)
            .ToArray();

        return tokens.Length == 0 ? string.Empty : string.Join(",", tokens);
    }

    private static bool TryBuildCachePayload(
        PagedResult<IEnumerable<Entity>> response,
        out CachedPositionsPage payload)
    {
        var data = response.Value;
        if (data is null)
        {
            payload = null!;
            return false;
        }

        var items = new List<Dictionary<string, object?>>();
        foreach (var entity in data)
        {
            if (entity is null)
            {
                continue;
            }

            var dict = new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase);
            foreach (var entry in entity)
            {
                dict[entry.Key] = entry.Value;
            }

            items.Add(dict);
        }

        payload = new CachedPositionsPage(
            items,
            response.PageNumber,
            response.PageSize,
            response.RecordsFiltered,
            response.RecordsTotal,
            response.Message,
            null);
        return true;
    }

    private static PagedResult<IEnumerable<Entity>> ToPagedResult(CachedPositionsPage payload)
    {
        var data = new List<Entity>(payload.Items.Count);
        foreach (var item in payload.Items)
        {
            var entity = new Entity();
            foreach (var entry in item)
            {
                entity.Add(entry.Key, entry.Value);
            }

            data.Add(entity);
        }

        var recordsCount = new RecordsCount
        {
            RecordsFiltered = payload.RecordsFiltered,
            RecordsTotal = payload.RecordsTotal
        };

        return PagedResult<IEnumerable<Entity>>.Success(data, payload.PageNumber, payload.PageSize, recordsCount, payload.Message);
    }

    private sealed record CachedPositionsPage(
        IReadOnlyList<Dictionary<string, object?>> Items,
        int PageNumber,
        int PageSize,
        int RecordsFiltered,
        int RecordsTotal,
        string? Message,
        DateTimeOffset? ExpiresAtUtc);
}
