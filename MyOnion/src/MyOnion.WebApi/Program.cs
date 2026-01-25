// Set up a try block to handle any exceptions during startup
try
{
    // Create a WebApplication builder with command-line arguments
    var builder = WebApplication.CreateBuilder(args);
    // Configure and initialize Serilog for logging
    Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .CreateLogger();

    // Use Serilog as the logging provider
    builder.Host.UseSerilog(Log.Logger);

    // Log information about application startup
    Log.Information("Application startup services registration");

    // Register application services
    builder.Services.AddApplicationLayer();
    builder.Services.AddPersistenceInfrastructure(builder.Configuration);
    builder.Services.AddSharedInfrastructure(builder.Configuration);
    builder.Services.AddEasyCachingInfrastructure(builder.Configuration);
    builder.Services.AddHttpContextAccessor();
    builder.Services.AddScoped<ICacheDiagnosticsPublisher, HttpCacheDiagnosticsPublisher>();
    builder.Services.AddSingleton<ICacheStatsCollector, CacheStatsCollector>();
    builder.Services.AddSwaggerExtension();
    builder.Services.AddControllersExtension(builder.Configuration);
    // Configure CORS policies
    builder.Services.AddCorsExtension(builder.Configuration, builder.Environment);
    // Add Health Checks service
    builder.Services.AddHealthChecks();
    builder.Services.AddFeatureManagement();
    builder.Services.AddScoped<IAuthorizationHandler, AuthEnabledRequirementHandler>();
    var authEnabled = builder.Configuration.GetSection("FeatureManagement").GetValue<bool>("AuthEnabled");
    var adminRole = builder.Configuration["ApiRoles:AdminRole"];
    var managerRole = builder.Configuration["ApiRoles:ManagerRole"];
    var employeeRole = builder.Configuration["ApiRoles:EmployeeRole"];
    builder.Services.AddAuthorization(options =>
    {
        options.DefaultPolicy = authEnabled
            ? new AuthorizationPolicyBuilder().AddRequirements(new AuthEnabledRequirement()).Build()
            : new AuthorizationPolicyBuilder().RequireAssertion(_ => true).Build();

        if (authEnabled)
        {
            options.AddPolicy(AuthorizationConsts.AdminPolicy, policy => policy.RequireRole(adminRole));
            options.AddPolicy(AuthorizationConsts.ManagerPolicy, policy => policy.RequireRole(managerRole, adminRole));
            options.AddPolicy(AuthorizationConsts.EmployeePolicy, policy => policy.RequireRole(employeeRole, managerRole, adminRole));
        }
        else
        {
            options.AddPolicy(AuthorizationConsts.AdminPolicy, policy => policy.RequireAssertion(_ => true));
            options.AddPolicy(AuthorizationConsts.ManagerPolicy, policy => policy.RequireAssertion(_ => true));
            options.AddPolicy(AuthorizationConsts.EmployeePolicy, policy => policy.RequireAssertion(_ => true));
        }
    });
    // Set up API security with JWT when auth is enabled
    if (authEnabled)
    {
        builder.Services.AddJWTAuthentication(builder.Configuration);
    }
    // Add API versioning extension
    builder.Services.AddApiVersioningExtension();
    // Add API explorer for Swagger
    builder.Services.AddMvcCore().AddApiExplorer();
    // Add versioned API explorer extension
    builder.Services.AddVersionedApiExplorerExtension();
    // Build the application
    var app = builder.Build();
    // Log information about middleware registration
    Log.Information("Application startup middleware registration");

    // Environment-specific configuration
    if (app.Environment.IsDevelopment())
    {
        // Use Developer Exception Page in development
        app.UseDeveloperExceptionPage();
        // Ensure the database is created and seed initial data during development
        using (var scope = app.Services.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            // Ensure the database is created and seed data if new
            if (dbContext.Database.EnsureCreated())
            {
                DbInitializer.SeedData(dbContext);
            }
        }
    }
    else
    {
        // Use Exception Handler and HSTS in non-development environments
        app.UseExceptionHandler("/Error");
        app.UseHsts();
    }
    // Log HTTP requests using Serilog
    app.UseSerilogRequestLogging();
    // Redirect HTTP requests to HTTPS
    app.UseHttpsRedirection();
    // Configure request routing
    app.UseRouting();
    app.UseRequestTimingMiddleware();
    // Enable configured CORS policy ("AllowAll" in this case)
    app.UseCors("AllowAll");
    // Use Authentication middleware
    app.UseAuthentication();
    app.UseCacheBypassMiddleware();
    // Use Authorization middleware
    app.UseAuthorization();
    // Enable Swagger for API documentation
    app.UseSwaggerExtension();
    // Use custom error handling middleware
    app.UseErrorHandlingMiddleware();
    // Configure Health Checks endpoint
    app.UseHealthChecks("/health");
    // Map controllers for endpoints
    app.MapControllers();
    // Log information that the application is starting
    Log.Information("Application Starting");
    // Run the application
    app.Run();
}
// Catch any exception that occurs during startup
catch (Exception ex)
{
    // Log warning with exception details
    Log.Warning(ex, "An error occurred starting the application");
    throw;
}
// Ensure the log is flushed properly
finally
{
    Log.CloseAndFlush();
}

/// <summary>
/// Partial Program class exposed for functional testing.
/// </summary>
public partial class Program { }
