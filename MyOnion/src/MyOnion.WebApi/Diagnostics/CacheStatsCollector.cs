#nullable enable
namespace MyOnion.WebApi.Diagnostics;

public sealed class CacheStatsCollector : ICacheStatsCollector
{
    private long _hits;
    private long _misses;
    private long _hitLatencyTicks;
    private long _missLatencyTicks;
    private readonly DateTimeOffset _startedAtUtc = DateTimeOffset.UtcNow;

    public void RecordHit(TimeSpan latency)
    {
        Interlocked.Increment(ref _hits);
        Interlocked.Add(ref _hitLatencyTicks, latency.Ticks);
    }

    public void RecordMiss(TimeSpan latency)
    {
        Interlocked.Increment(ref _misses);
        Interlocked.Add(ref _missLatencyTicks, latency.Ticks);
    }

    public CacheStatsSnapshot Snapshot()
    {
        var hits = Interlocked.Read(ref _hits);
        var misses = Interlocked.Read(ref _misses);
        var hitTicks = Interlocked.Read(ref _hitLatencyTicks);
        var missTicks = Interlocked.Read(ref _missLatencyTicks);
        var averageHitMs = hits == 0 ? 0 : TimeSpan.FromTicks(hitTicks / hits).TotalMilliseconds;
        var averageMissMs = misses == 0 ? 0 : TimeSpan.FromTicks(missTicks / misses).TotalMilliseconds;
        return new CacheStatsSnapshot(hits, misses, averageHitMs, averageMissMs, _startedAtUtc);
    }
}
