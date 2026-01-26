using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using System.Security.Claims;

namespace MyOnion.WebApi.Tests.Caching;

public class CacheBypassMiddlewareTests
{
    [Fact]
    public async Task InvokeAsync_WhenHeaderAndAdminRole_EnableBypass()
    {
        var options = new CachingOptions { Enabled = true };
        var optionsMonitor = new Mock<IOptionsMonitor<CachingOptions>>();
        optionsMonitor.SetupGet(o => o.CurrentValue).Returns(options);

        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ApiRoles:AdminRole"] = "Admin"
            })
            .Build();

        var logger = new Mock<ILogger<CacheBypassMiddleware>>();
        var bypassContext = new TestCacheBypassContext();

        var context = new DefaultHttpContext();
        context.Request.Headers[CacheBypassMiddleware.HeaderName] = "true";
        context.User = new ClaimsPrincipal(new ClaimsIdentity(new[]
        {
            new Claim(ClaimTypes.Role, "Admin")
        }, "test"));

        var invokedWithBypass = false;
        Task Next(HttpContext _)
        {
            invokedWithBypass = bypassContext.ShouldBypass;
            return Task.CompletedTask;
        }

        var middleware = new CacheBypassMiddleware(Next, optionsMonitor.Object, logger.Object, configuration);
        await middleware.InvokeAsync(context, bypassContext);

        invokedWithBypass.Should().BeTrue();
        bypassContext.ShouldBypass.Should().BeFalse();
        bypassContext.Reason.Should().BeNull();
    }

    private sealed class TestCacheBypassContext : ICacheBypassContext
    {
        public bool ShouldBypass { get; private set; }
        public string? Reason { get; private set; }

        public void Enable(string reason)
        {
            ShouldBypass = true;
            Reason = reason;
        }

        public void Reset()
        {
            ShouldBypass = false;
            Reason = null;
        }
    }
}
