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
        var distributedConnectionString = section.GetSection(nameof(CachingOptions.ProviderSettings))
            .Get<ProviderSettings>()?.Distributed?.ConnectionString;
        services.AddMemoryCache(options =>
        {
            var settings = section.GetSection(nameof(CachingOptions.ProviderSettings)).Get<ProviderSettings>() ?? new ProviderSettings();
            if (settings.Memory.SizeLimitMB is int sizeLimit && sizeLimit > 0)
            {
                options.SizeLimit = sizeLimit * 1024L * 1024L;
            }
        });
        if (!string.IsNullOrWhiteSpace(distributedConnectionString))
        {
            services.AddStackExchangeRedisCache(options =>
            {
                options.Configuration = distributedConnectionString;
            });
        }

        services.AddScoped<ICacheBypassContext, CacheBypassContext>();
        services.AddSingleton<ICacheKeyHasher, CacheKeyHasher>();
        services.AddSingleton<ICacheEntryOptionsFactory, CacheEntryOptionsFactory>();
        services.AddScoped<ICacheInvalidationService, CacheInvalidationService>();
        services.AddScoped<ICacheKeyIndex>(sp =>
        {
            var optionsMonitor = sp.GetRequiredService<IOptionsMonitor<CachingOptions>>();
            var provider = optionsMonitor.CurrentValue.Provider;
            if (string.Equals(provider, CacheProviders.Distributed, StringComparison.OrdinalIgnoreCase))
            {
                var distributedCache = sp.GetService<IDistributedCache>()
                    ?? throw new InvalidOperationException("IDistributedCache is not registered but the Distributed provider is selected.");
                return new DistributedCacheKeyIndex(distributedCache, sp.GetRequiredService<ICacheKeyHasher>(), optionsMonitor);
            }

            var memoryCache = sp.GetRequiredService<IMemoryCache>();
            return new MemoryCacheKeyIndex(memoryCache, sp.GetRequiredService<ICacheKeyHasher>(), optionsMonitor);
        });
        services.AddScoped<ICacheProvider>(sp =>
        {
            var optionsMonitor = sp.GetRequiredService<IOptionsMonitor<CachingOptions>>();
            var provider = optionsMonitor.CurrentValue.Provider;
            if (string.Equals(provider, CacheProviders.Distributed, StringComparison.OrdinalIgnoreCase))
            {
                var distributedCache = sp.GetService<IDistributedCache>()
                    ?? throw new InvalidOperationException("IDistributedCache is not registered but the Distributed provider is selected.");
                return new DistributedCacheProvider(
                    distributedCache,
                    optionsMonitor,
                    sp.GetRequiredService<ICacheBypassContext>(),
                    sp.GetRequiredService<ICacheKeyIndex>());
            }

            var memoryCache = sp.GetRequiredService<IMemoryCache>();
            return new MemoryCacheProvider(
                memoryCache,
                optionsMonitor,
                sp.GetRequiredService<ICacheBypassContext>(),
                sp.GetRequiredService<ICacheKeyIndex>());
        });

        return services;
    }
}
