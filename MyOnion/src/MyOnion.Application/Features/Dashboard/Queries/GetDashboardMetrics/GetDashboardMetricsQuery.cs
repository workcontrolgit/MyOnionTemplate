#nullable enable
using System.Diagnostics;
using MyOnion.Application.Common.Caching;
using MyOnion.Application.Interfaces;
using MyOnion.Application.Interfaces.Caching;

namespace MyOnion.Application.Features.Dashboard.Queries.GetDashboardMetrics;

public sealed class GetDashboardMetricsQuery : IRequest<Result<DashboardMetricsDto>>
{
    public sealed class GetDashboardMetricsQueryHandler
        : IRequestHandler<GetDashboardMetricsQuery, Result<DashboardMetricsDto>>
    {
        private readonly IDashboardMetricsReader _reader;
        private readonly ICacheProvider _cacheProvider;
        private readonly ICacheEntryOptionsFactory _entryOptionsFactory;
        private readonly ICacheDiagnosticsPublisher _diagnosticsPublisher;
        private readonly ICacheStatsCollector _statsCollector;

        public GetDashboardMetricsQueryHandler(
            IDashboardMetricsReader reader,
            ICacheProvider cacheProvider,
            ICacheEntryOptionsFactory entryOptionsFactory,
            ICacheDiagnosticsPublisher diagnosticsPublisher,
            ICacheStatsCollector statsCollector)
        {
            _reader = reader;
            _cacheProvider = cacheProvider;
            _entryOptionsFactory = entryOptionsFactory;
            _diagnosticsPublisher = diagnosticsPublisher;
            _statsCollector = statsCollector;
        }

        public async Task<Result<DashboardMetricsDto>> Handle(
            GetDashboardMetricsQuery request,
            CancellationToken cancellationToken)
        {
            var cacheKey = CacheKeyPrefixes.DashboardMetrics;
            var entryOptions = _entryOptionsFactory.Create(cacheKey);
            var start = Stopwatch.GetTimestamp();
            var cached = await _cacheProvider.GetAsync<CachedDashboardMetrics>(cacheKey, cancellationToken).ConfigureAwait(false);
            var latency = Stopwatch.GetElapsedTime(start);
            if (cached is not null)
            {
                var remainingTtl = cached.ExpiresAtUtc is { } expiresAt
                    ? expiresAt - DateTimeOffset.UtcNow
                    : (TimeSpan?)null;
                _diagnosticsPublisher.ReportHit(cacheKey, remainingTtl);
                _statsCollector.RecordHit(latency);
                return Result<DashboardMetricsDto>.Success(cached.Payload);
            }

            var dto = await _reader.GetDashboardMetricsAsync(cancellationToken).ConfigureAwait(false);
            _diagnosticsPublisher.ReportMiss(cacheKey, null);
            _statsCollector.RecordMiss(latency);
            var payload = new CachedDashboardMetrics(
                dto,
                DateTimeOffset.UtcNow.Add(entryOptions.AbsoluteTtl));
            await _cacheProvider.SetAsync(cacheKey, payload, entryOptions, cancellationToken).ConfigureAwait(false);
            return Result<DashboardMetricsDto>.Success(dto);
        }

        private sealed record CachedDashboardMetrics(
            DashboardMetricsDto Payload,
            DateTimeOffset? ExpiresAtUtc);
    }
}
