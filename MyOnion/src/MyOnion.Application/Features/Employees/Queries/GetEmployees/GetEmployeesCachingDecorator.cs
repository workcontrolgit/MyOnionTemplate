#nullable enable
using System.Text;
using MyOnion.Application.Interfaces.Caching;

namespace MyOnion.Application.Features.Employees.Queries.GetEmployees;

public sealed class GetEmployeesCachingDecorator : IRequestHandler<GetEmployeesQuery, PagedResult<IEnumerable<Entity>>>
{
    private static string EndpointKey => CacheKeyPrefixes.EmployeesAll;
    private readonly IRequestHandler<GetEmployeesQuery, PagedResult<IEnumerable<Entity>>> _inner;
    private readonly ICacheProvider _cacheProvider;
    private readonly ICacheEntryOptionsFactory _entryOptionsFactory;
    private readonly ICacheDiagnosticsPublisher _diagnosticsPublisher;

    public GetEmployeesCachingDecorator(
        IRequestHandler<GetEmployeesQuery, PagedResult<IEnumerable<Entity>>> inner,
        ICacheProvider cacheProvider,
        ICacheEntryOptionsFactory entryOptionsFactory,
        ICacheDiagnosticsPublisher diagnosticsPublisher)
    {
        _inner = inner;
        _cacheProvider = cacheProvider;
        _entryOptionsFactory = entryOptionsFactory;
        _diagnosticsPublisher = diagnosticsPublisher;
    }

    public async Task<PagedResult<IEnumerable<Entity>>> Handle(GetEmployeesQuery request, CancellationToken cancellationToken)
    {
        var cacheKey = BuildCacheKey(request);
        var cachedResponse = await _cacheProvider.GetAsync<PagedResult<IEnumerable<Entity>>>(cacheKey, cancellationToken).ConfigureAwait(false);
        if (cachedResponse is not null)
        {
            _diagnosticsPublisher.ReportHit();
            return cachedResponse;
        }

        var response = await _inner.Handle(request, cancellationToken).ConfigureAwait(false);
        if (!response.IsSuccess)
        {
            return response;
        }

        _diagnosticsPublisher.ReportMiss();
        var entryOptions = _entryOptionsFactory.Create(EndpointKey);
        await _cacheProvider.SetAsync(cacheKey, response, entryOptions, cancellationToken).ConfigureAwait(false);
        return response;
    }

    private static string BuildCacheKey(GetEmployeesQuery request)
    {
        var builder = new StringBuilder(EndpointKey);
        builder.Append(":page=").Append(request.PageNumber);
        builder.Append(":size=").Append(request.PageSize);
        AppendFilter(builder, "last", request.LastName);
        AppendFilter(builder, "first", request.FirstName);
        AppendFilter(builder, "email", request.Email);
        AppendFilter(builder, "number", request.EmployeeNumber);
        AppendFilter(builder, "position", request.PositionTitle);
        AppendFilter(builder, "fields", request.Fields);
        AppendFilter(builder, "order", request.ShapeParameter?.OrderBy);
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
}
