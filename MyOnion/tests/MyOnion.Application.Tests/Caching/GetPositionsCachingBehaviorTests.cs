using System.Collections.Concurrent;

namespace MyOnion.Application.Tests.Caching;

public class GetPositionsCachingBehaviorTests
{
    [Fact]
    public async Task Handle_WhenCacheHit_ReturnsCachedPayloadAndSkipsNext()
    {
        var cacheProvider = new TestCacheProvider
        {
            GetHandler = type => CreateCachedPayload(type)
        };
        var entryOptionsFactory = new TestEntryOptionsFactory();
        var diagnostics = new Mock<ICacheDiagnosticsPublisher>();
        var stats = new Mock<ICacheStatsCollector>();

        var behavior = new GetPositionsCachingBehavior(
            cacheProvider,
            entryOptionsFactory,
            diagnostics.Object,
            stats.Object);

        var request = new GetPositionsQuery { PageNumber = 1, PageSize = 10 };
        var nextCalled = false;
        Task<PagedResult<IEnumerable<Entity>>> Next()
        {
            nextCalled = true;
            return Task.FromResult(PagedResult<IEnumerable<Entity>>.Failure("nope", 1, 10));
        }

        var result = await behavior.Handle(request, Next, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        nextCalled.Should().BeFalse();
        cacheProvider.SetCalls.Should().BeEmpty();
        diagnostics.Verify(d => d.ReportHit(It.IsAny<string>(), It.IsAny<TimeSpan?>()), Times.Once);
        diagnostics.Verify(d => d.ReportMiss(It.IsAny<string>(), It.IsAny<TimeSpan?>()), Times.Never);
        stats.Verify(s => s.RecordHit(It.IsAny<TimeSpan>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenCacheMiss_CachesResponse()
    {
        var cacheProvider = new TestCacheProvider
        {
            GetHandler = _ => null
        };
        var entryOptionsFactory = new TestEntryOptionsFactory();
        var diagnostics = new Mock<ICacheDiagnosticsPublisher>();
        var stats = new Mock<ICacheStatsCollector>();

        var behavior = new GetPositionsCachingBehavior(
            cacheProvider,
            entryOptionsFactory,
            diagnostics.Object,
            stats.Object);

        var request = new GetPositionsQuery { PageNumber = 1, PageSize = 10 };
        var nextCalled = false;
        Task<PagedResult<IEnumerable<Entity>>> Next()
        {
            nextCalled = true;
            var items = new List<Entity> { new() { ["Id"] = Guid.NewGuid() } };
            var counts = new RecordsCount { RecordsFiltered = 1, RecordsTotal = 1 };
            return Task.FromResult(PagedResult<IEnumerable<Entity>>.Success(items, 1, 10, counts));
        }

        var result = await behavior.Handle(request, Next, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        nextCalled.Should().BeTrue();
        cacheProvider.SetCalls.Should().HaveCount(1);
        diagnostics.Verify(d => d.ReportMiss(It.IsAny<string>(), It.IsAny<TimeSpan?>()), Times.Once);
        stats.Verify(s => s.RecordMiss(It.IsAny<TimeSpan>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenPayloadInvalid_DoesNotCache()
    {
        var cacheProvider = new TestCacheProvider
        {
            GetHandler = _ => null
        };
        var entryOptionsFactory = new TestEntryOptionsFactory();
        var diagnostics = new Mock<ICacheDiagnosticsPublisher>();
        var stats = new Mock<ICacheStatsCollector>();

        var behavior = new GetPositionsCachingBehavior(
            cacheProvider,
            entryOptionsFactory,
            diagnostics.Object,
            stats.Object);

        var request = new GetPositionsQuery { PageNumber = 1, PageSize = 10 };
        Task<PagedResult<IEnumerable<Entity>>> Next()
        {
            return Task.FromResult(PagedResult<IEnumerable<Entity>>.Success(null, 1, 10, new RecordsCount()));
        }

        var result = await behavior.Handle(request, Next, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        cacheProvider.SetCalls.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_BuildsNormalizedCacheKey()
    {
        var cacheProvider = new TestCacheProvider
        {
            GetHandler = _ => null
        };
        var entryOptionsFactory = new TestEntryOptionsFactory();
        var diagnostics = new Mock<ICacheDiagnosticsPublisher>();
        var stats = new Mock<ICacheStatsCollector>();

        var behavior = new GetPositionsCachingBehavior(
            cacheProvider,
            entryOptionsFactory,
            diagnostics.Object,
            stats.Object);

        var request = new GetPositionsQuery
        {
            PageNumber = 3,
            PageSize = 5,
            PositionTitle = "  QA Engineer ",
            Department = "  Engineering ",
            Fields = "Id, PositionTitle"
        };

        Task<PagedResult<IEnumerable<Entity>>> Next()
        {
            var items = new List<Entity> { new() { ["Id"] = Guid.NewGuid() } };
            var counts = new RecordsCount { RecordsFiltered = 1, RecordsTotal = 1 };
            return Task.FromResult(PagedResult<IEnumerable<Entity>>.Success(items, 3, 5, counts));
        }

        await behavior.Handle(request, Next, CancellationToken.None);

        cacheProvider.SetCalls.Should().ContainSingle();
        cacheProvider.SetCalls.TryPeek(out var call).Should().BeTrue();
        call.Key.Should().Contain("page=3");
        call.Key.Should().Contain("size=5");
        call.Key.Should().Contain("title=qa engineer");
        call.Key.Should().Contain("department=engineering");
        call.Key.Should().Contain("fields=id,positiontitle");
    }

    private static object CreateCachedPayload(Type type)
    {
        var items = new List<Dictionary<string, object?>>
        {
            new(StringComparer.OrdinalIgnoreCase)
            {
                ["Id"] = Guid.NewGuid(),
                ["PositionTitle"] = "Engineer"
            }
        };

        return Activator.CreateInstance(
            type,
            items,
            1,
            10,
            1,
            1,
            "cached",
            DateTimeOffset.UtcNow.AddMinutes(5))!;
    }

    private sealed class TestEntryOptionsFactory : ICacheEntryOptionsFactory
    {
        public CacheEntryOptions Create(string endpointKey)
            => new(TimeSpan.FromMinutes(5));
    }

    private sealed class TestCacheProvider : ICacheProvider
    {
        public Func<Type, object?>? GetHandler { get; init; }
        public ConcurrentQueue<(string Key, object Value, CacheEntryOptions Options)> SetCalls { get; } = new();

        public Task<T?> GetAsync<T>(string key, CancellationToken ct = default)
        {
            var value = GetHandler?.Invoke(typeof(T));
            return Task.FromResult((T?)value);
        }

        public Task SetAsync<T>(string key, T value, CacheEntryOptions options, CancellationToken ct = default)
        {
            SetCalls.Enqueue((key, value!, options));
            return Task.CompletedTask;
        }

        public Task RemoveAsync(string key, CancellationToken ct = default) => Task.CompletedTask;

        public Task RemoveByPrefixAsync(string prefix, CancellationToken ct = default) => Task.CompletedTask;
    }
}
