using EasyCaching.Serialization.SystemTextJson.Configurations;
using MyOnion.WebApi.Caching.Options;
using MyOnion.WebApi.Caching.Services;

namespace MyOnion.WebApi.Caching;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddEasyCachingInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var section = configuration.GetSection(CachingOptions.SectionName);
        services.Configure<CachingOptions>(section);
        var providerSettings = section.GetSection(nameof(CachingOptions.ProviderSettings)).Get<ProviderSettings>() ?? new ProviderSettings();

        services.AddEasyCaching(options =>
        {
            options.WithSystemTextJson();
            options.UseInMemory(config =>
            {
            }, CacheProviderNames.Memory);

            var connectionString = providerSettings.Distributed.ConnectionString;
            var provider = section.GetValue<string>(nameof(CachingOptions.Provider)) ?? CacheProviders.Memory;
            if (string.Equals(provider, CacheProviders.Distributed, StringComparison.OrdinalIgnoreCase)
                && !string.IsNullOrWhiteSpace(connectionString))
            {
                options.UseRedis(config =>
                {
                    var normalized = connectionString.Contains("abortConnect", StringComparison.OrdinalIgnoreCase)
                        || connectionString.Contains("AbortOnConnectFail", StringComparison.OrdinalIgnoreCase)
                        ? connectionString
                        : string.Concat(connectionString, ",abortConnect=false");
                    config.DBConfig.Configuration = normalized;
                }, CacheProviderNames.Redis);
            }
        });

        services.AddScoped<ICacheBypassContext, CacheBypassContext>();
        services.AddSingleton<ICacheKeyHasher, CacheKeyHasher>();
        services.AddSingleton<ICacheEntryOptionsFactory, CacheEntryOptionsFactory>();
        services.AddScoped<ICacheKeyIndex, EasyCachingKeyIndex>();
        services.AddScoped<ICacheInvalidationService, CacheInvalidationService>();
        services.AddScoped<ICacheProvider, EasyCachingProviderAdapter>();

        return services;
    }

    private static class CacheProviderNames
    {
        public const string Memory = "mem";
        public const string Redis = "redis";
    }
}
