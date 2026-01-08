using System;
using MyOnion.Infrastructure.Caching.Options;
using MyOnion.Infrastructure.Caching.Providers.Distributed;
using MyOnion.Infrastructure.Caching.Providers.Memory;
using MyOnion.Infrastructure.Caching.Services;

namespace MyOnion.Infrastructure.Caching.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddCachingInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var section = configuration.GetSection(CachingOptions.SectionName);
        services.Configure<CachingOptions>(section);
        services.AddMemoryCache(options =>
        {
            var settings = section.GetSection(nameof(CachingOptions.ProviderSettings)).Get<ProviderSettings>() ?? new ProviderSettings();
            if (settings.Memory.SizeLimitMB is int sizeLimit && sizeLimit > 0)
            {
                options.SizeLimit = sizeLimit * 1024L * 1024L;
            }
        });

        services.AddScoped<ICacheBypassContext, CacheBypassContext>();
        services.AddSingleton<ICacheEntryOptionsFactory, CacheEntryOptionsFactory>();
        services.AddScoped<ICacheInvalidationService, CacheInvalidationService>();
        services.AddScoped<ICacheProvider>(sp =>
        {
            var optionsMonitor = sp.GetRequiredService<IOptionsMonitor<CachingOptions>>();
            var provider = optionsMonitor.CurrentValue.Provider;
            if (string.Equals(provider, CacheProviders.Distributed, StringComparison.OrdinalIgnoreCase))
            {
                var distributedCache = sp.GetService<IDistributedCache>()
                    ?? throw new InvalidOperationException("IDistributedCache is not registered but the Distributed provider is selected.");
                return new DistributedCacheProvider(distributedCache, optionsMonitor, sp.GetRequiredService<ICacheBypassContext>());
            }

            var memoryCache = sp.GetRequiredService<IMemoryCache>();
            return new MemoryCacheProvider(memoryCache, optionsMonitor, sp.GetRequiredService<ICacheBypassContext>());
        });

        return services;
    }
}
