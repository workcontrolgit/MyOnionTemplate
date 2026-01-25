using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;

namespace MyOnion.WebApi.Tests.Caching;

public class HttpCacheDiagnosticsPublisherTests
{
    [Fact]
    public void ReportHit_WritesCacheHeaders()
    {
        var options = new CachingOptions
        {
            Diagnostics = new CacheDiagnosticsOptions
            {
                EmitCacheStatusHeader = true,
                KeyDisplayMode = CacheKeyDisplayModes.Hash
            }
        };

        var optionsMonitor = new Mock<IOptionsMonitor<CachingOptions>>();
        optionsMonitor.SetupGet(o => o.CurrentValue).Returns(options);

        var context = new DefaultHttpContext();
        var accessor = new HttpContextAccessor { HttpContext = context };
        var hasher = new Mock<ICacheKeyHasher>();
        hasher.Setup(h => h.Hash("cache-key")).Returns("hashed-key");

        var publisher = new HttpCacheDiagnosticsPublisher(accessor, optionsMonitor.Object, hasher.Object);

        publisher.ReportHit("cache-key", TimeSpan.FromMilliseconds(25));

        context.Response.Headers[CacheDiagnosticsOptions.DefaultHeaderName].ToString().Should().Be("HIT");
        context.Response.Headers[CacheDiagnosticsOptions.DefaultKeyHeaderName].ToString().Should().Be("hashed-key");
        context.Response.Headers[CacheDiagnosticsOptions.DefaultDurationHeaderName].ToString().Should().Be("25");
    }
}
