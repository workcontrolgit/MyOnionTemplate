# Feature Management in Template OnionAPI v10.2.0

Template OnionAPI ships as a Visual Studio template and stays committed to flexible, environment-aware configuration. Download the template here: https://marketplace.visualstudio.com/items?itemName=workcontrol.VSIXTemplateOnionAPI. For the Template OnionAPI complete sourc code visit https://github.com/workcontrolgit/MyOnionTemplate

Version 10.2.0 introduces `Microsoft.FeatureManagement` to enable runtime feature toggles across authentication, caching, and execution timing. This allows developers to run the template in tutorial mode without JWT configuration while keeping production secured, and provides granular control over diagnostic features without redeployment.

## What Feature Management Does

1. **Runtime Feature Toggles** - Features can be enabled or disabled through configuration without code changes or redeployment.
2. **Environment-Aware Configuration** - Tutorial environments can disable authentication while production enforces it by default.
3. **Diagnostic Control** - Cache diagnostics headers and execution timing can be toggled independently for development vs. production.
4. **Startup-Time Decisions** - Database provider selection (in-memory vs. SQL Server) is determined at startup via feature flags.

## Implemented Feature Flags

Template OnionAPI v10.2.0 includes the following feature flags in `appsettings.json`:

```json
{
  "FeatureManagement": {
    "AuthEnabled": true,
    "CacheEnabled": true,
    "CacheDiagnosticsHeaders": false,
    "ExecutionTimingEnabled": true,
    "ExecutionTimingIncludeHeader": true,
    "ExecutionTimingIncludePayload": true,
    "ExecutionTimingLogTimings": false,
    "UseInMemoryDatabase": false
  }
}
```

- **AuthEnabled**: Controls authentication enforcement. When `false`, allows anonymous access for tutorial/demo environments.
- **CacheEnabled**: Master toggle for EasyCaching functionality.
- **CacheDiagnosticsHeaders**: Toggles cache status headers (`X-Cache-Status`, `X-Cache-Key`, `X-Cache-Duration-Ms`).
- **ExecutionTimingEnabled**: Controls request timing middleware and filters.
- **ExecutionTimingIncludeHeader**: Toggles `x-execution-time-ms` response header.
- **ExecutionTimingIncludePayload**: Controls whether execution time is included in result payload.
- **ExecutionTimingLogTimings**: Toggles execution timing log entries.
- **UseInMemoryDatabase**: Switches between SQL Server and in-memory database (startup-only).

## Why Feature Management Matters

- **Tutorial Mode** - New users can explore the API immediately without setting up JWT or an identity provider.
- **Environment-Specific Behavior** - Development can show diagnostics while production hides them, all from configuration.
- **Safe Deployment** - Features can be deployed disabled and enabled later without code changes.
- **Reduced Configuration Complexity** - Single source of truth for feature toggles replaces scattered boolean flags.

## Example Code

### Authentication Control

The `AuthEnabledRequirement` handler checks the feature flag before enforcing authentication:

```csharp
public sealed class AuthEnabledRequirementHandler : AuthorizationHandler<AuthEnabledRequirement>
{
    private readonly IFeatureManagerSnapshot _featureManager;

    public AuthEnabledRequirementHandler(IFeatureManagerSnapshot featureManager)
    {
        _featureManager = featureManager;
    }

    protected override async Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        AuthEnabledRequirement requirement)
    {
        var enabled = await _featureManager.IsEnabledAsync("AuthEnabled").ConfigureAwait(false);
        if (!enabled)
        {
            context.Succeed(requirement);
            return;
        }

        if (context.User?.Identity?.IsAuthenticated == true)
        {
            context.Succeed(requirement);
        }
    }
}
```
Source: `MyOnion/src/MyOnion.WebApi/Authorization/AuthEnabledRequirement.cs:9`

When `AuthEnabled` is `false`, all requests are allowed through regardless of authentication state. When `true`, standard authentication rules apply.

### Cache Control

The caching adapter checks the feature flag before performing cache operations:

```csharp
private bool IsCacheEnabled(CachingOptions options)
{
    var featureEnabled = _featureManager.IsEnabledAsync("CacheEnabled").GetAwaiter().GetResult();
    return featureEnabled && options.Enabled && !options.DisableCache && !_bypassContext.ShouldBypass;
}
```
Source: `MyOnion/src/MyOnion.WebApi/Caching/Services/EasyCachingProviderAdapter.cs:74`

This creates a layered approach where the feature flag acts as a master switch, while individual cache configuration settings provide fine-grained control.

### Cache Diagnostics Headers

Cache diagnostics headers are controlled by both configuration and feature flags:

```csharp
public void PublishCacheEvent(CacheDiagnosticEvent diagnosticEvent)
{
    var diagnostics = _optionsMonitor.CurrentValue.Diagnostics;
    var diagnosticsEnabled = _featureManager.IsEnabledAsync("CacheDiagnosticsHeaders").GetAwaiter().GetResult();
    if (diagnostics is null || !diagnostics.EmitCacheStatusHeader || !diagnosticsEnabled)
    {
        return;
    }

    // Emit cache status headers (X-Cache-Status, X-Cache-Key, etc.)
}
```
Source: `MyOnion/src/MyOnion.WebApi/Diagnostics/HttpCacheDiagnosticsPublisher.cs:37`

### Execution Timing Toggle

The execution timing middleware respects the feature flag:

```csharp
var enabled = await _featureManager.IsEnabledAsync("ExecutionTimingEnabled").ConfigureAwait(false);
if (!enabled)
{
    await _next(context).ConfigureAwait(false);
    return;
}

// Proceed with timing logic
```
Source: `MyOnion/src/MyOnion.WebApi/Middlewares/RequestTimingMiddleware.cs:31`

### Database Provider Selection

Database provider is chosen at startup based on the feature flag:

```csharp
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
            sqlOptions => { /* ... */ });
        ConfigureCommonOptions(provider, options);
    });
}
```
Source: `MyOnion/src/MyOnion.Infrastructure.Persistence/ServiceRegistration.cs:9`

## Authorization Policy Integration

Feature management integrates with ASP.NET Core authorization policies:

```csharp
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
```
Source: `MyOnion/src/MyOnion.WebApi/Program.cs:38`

## Tutorial Mode Example

To run the template in tutorial mode without JWT setup, simply set `AuthEnabled` to `false` in `appsettings.Development.json`:

```json
{
  "FeatureManagement": {
    "AuthEnabled": false
  }
}
```

All endpoints become accessible without authentication, perfect for demos, learning, and rapid prototyping. Switch it back to `true` for production deployment.

## Production Best Practices

1. **Keep AuthEnabled true in production** - Only disable authentication in controlled tutorial/demo environments.
2. **Disable diagnostics headers in production** - Set `CacheDiagnosticsHeaders: false` to prevent key exposure.
3. **Use environment-specific configuration** - Override flags in `appsettings.Development.json` or environment variables.
4. **Test both modes** - Verify that features work correctly when enabled and disabled.
5. **Avoid runtime database provider switching** - `UseInMemoryDatabase` should only be toggled at startup, not runtime.

## Blog Summary

- Template OnionAPI v10.2.0 introduces `Microsoft.FeatureManagement` for runtime feature toggles.
- Eight feature flags control authentication, caching, diagnostics, and execution timing.
- Tutorial mode allows anonymous access for learning without JWT configuration.
- Feature flags integrate with authorization policies, middleware, and service registration.
- Environment-specific configuration keeps development diagnostic-rich and production secure.
