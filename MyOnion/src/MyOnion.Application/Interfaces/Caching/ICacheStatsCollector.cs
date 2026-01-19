namespace MyOnion.Application.Interfaces.Caching;

public interface ICacheStatsCollector
{
    void RecordHit(TimeSpan latency);
    void RecordMiss(TimeSpan latency);
    CacheStatsSnapshot Snapshot();
}

public sealed record CacheStatsSnapshot(
    long Hits,
    long Misses,
    double AverageHitLatencyMs,
    double AverageMissLatencyMs,
    DateTimeOffset StartedAtUtc);
