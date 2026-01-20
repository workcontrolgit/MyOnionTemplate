using System.Reflection;
using MyOnion.Infrastructure.Persistence.Readers;
namespace MyOnion.Infrastructure.Persistence
{
    public static class ServiceRegistration
    {
        public static void AddPersistenceInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {
            if (configuration.GetValue<bool>("UseInMemoryDatabase"))
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
            options.UseLoggerFactory(loggerFactory)
                .EnableSensitiveDataLogging()
                .EnableDetailedErrors();
        }
    }
}

