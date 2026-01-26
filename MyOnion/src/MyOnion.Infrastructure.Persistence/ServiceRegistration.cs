using System.Reflection;
using MyOnion.Infrastructure.Persistence.Readers;
namespace MyOnion.Infrastructure.Persistence
{
    public static class ServiceRegistration
    {
        public static void AddPersistenceInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {
            var featureUseInMemory = configuration.GetSection("FeatureManagement").GetValue<bool?>("UseInMemoryDatabase");
            var useInMemory = featureUseInMemory ?? configuration.GetValue<bool>("UseInMemoryDatabase");
            if (useInMemory)
            {
                services.AddDbContext<ApplicationDbContext>((provider, options) =>
                {
                    options.UseInMemoryDatabase("ApplicationDb");
                    ConfigureCommonOptions(provider, options);
                });
            }
            else
            {
                services.AddDbContextPool<ApplicationDbContext>((provider, options) =>
                {
                    options.UseSqlServer(
                        configuration.GetConnectionString("DefaultConnection"),
                        sqlOptions =>
                        {
                            sqlOptions.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName);
                            sqlOptions.EnableRetryOnFailure(
                                maxRetryCount: 5,
                                maxRetryDelay: TimeSpan.FromSeconds(15),
                                errorNumbersToAdd: null);
                        });
                    ConfigureCommonOptions(provider, options);
                });
            }

            #region Repositories

            // * use Scutor to register generic repository interface for DI and specifying the lifetime of dependencies
            services.Scan(selector => selector
                .FromAssemblies(Assembly.GetExecutingAssembly())
                .AddClasses(classSelector => classSelector.AssignableTo(typeof(IGenericRepositoryAsync<>)))
                .AsImplementedInterfaces()
                .WithTransientLifetime()
                );

            #endregion Repositories

            services.AddScoped<IDashboardMetricsReader, DashboardMetricsReader>();
        }

        private static void ConfigureCommonOptions(IServiceProvider provider, DbContextOptionsBuilder options)
        {
            var loggerFactory = provider.GetRequiredService<ILoggerFactory>();
            var environment = provider.GetRequiredService<IHostEnvironment>();
            var builder = options.UseLoggerFactory(loggerFactory)
                .EnableDetailedErrors();

            if (environment.IsDevelopment())
            {
                builder.EnableSensitiveDataLogging();
            }
        }
    }
}

