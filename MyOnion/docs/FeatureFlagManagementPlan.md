# Feature Flag Management Plan

## Goal
Use `Microsoft.FeatureManagement` to control authentication with a feature flag so the tutorial UI can run without JWT configuration while production remains secured. Expand feature management to cover selected runtime toggles currently in appsettings.

## Use Case: Tutorial Mode
For onboarding and demo environments, the tutorial UI should access the API without setting up JWT or an identity provider. The `AuthEnabled` feature flag allows developers to disable authentication quickly in these environments, while keeping it enforced everywhere else by default.

## Recommended Flags
- **AuthEnabled**: When `true`, require authentication. When `false`, allow anonymous access.
- **CacheEnabled**: Toggle `Caching.Enabled`.
- **CacheDiagnosticsHeaders**: Toggle `Caching.Diagnostics.EmitCacheStatusHeader`.
- **ExecutionTimingEnabled**: Toggle `ExecutionTiming.Enabled`.
- **ExecutionTimingIncludeHeader**: Toggle `ExecutionTiming.IncludeHeader`.
- **ExecutionTimingIncludePayload**: Toggle `ExecutionTiming.IncludeResultPayload`.
- **ExecutionTimingLogTimings**: Toggle `ExecutionTiming.LogTimings`.
- **UseInMemoryDatabase**: Toggle `UseInMemoryDatabase` (dev-only guard).

## Task Checklist

### 1) Add Feature Management
- [ ] Add NuGet package: `Microsoft.FeatureManagement.AspNetCore`.
- [ ] Register feature management in `Program.cs`.

### 2) Configuration
- [ ] Add `FeatureManagement` section to `appsettings.json`:
```json
{
  "FeatureManagement": {
    "AuthEnabled": true,
    "CacheEnabled": true,
    "CacheDiagnosticsHeaders": false,
    "ExecutionTimingEnabled": true,
    "ExecutionTimingIncludeHeader": true,
    "ExecutionTimingIncludePayload": false,
    "ExecutionTimingLogTimings": false,
    "UseInMemoryDatabase": false
  }
}
```
- [ ] For tutorial environments, set `AuthEnabled` to `false` in `appsettings.Development.json` or user secrets.
 - [ ] Align existing appsettings values with flags (document precedence if both exist).

### 3) Authorization Wiring (Sample)
Implement a fallback policy that only enforces auth when the flag is enabled:
```csharp
using Microsoft.FeatureManagement;
using Microsoft.FeatureManagement.Mvc;
using Microsoft.AspNetCore.Authorization;

builder.Services.AddFeatureManagement();

builder.Services.AddAuthorization(options =>
{
    options.FallbackPolicy = new AuthorizationPolicyBuilder()
        .RequireAssertion(async context =>
        {
            var featureManager = context.Resource switch
            {
                HttpContext http => http.RequestServices.GetRequiredService<IFeatureManagerSnapshot>(),
                _ => null
            };

            if (featureManager is null)
            {
                return false;
            }

            var authEnabled = await featureManager.IsEnabledAsync("AuthEnabled");
            return !authEnabled || context.User.Identity?.IsAuthenticated == true;
        })
        .Build();
});
```

### 4) Controller Usage
- [ ] Keep `[Authorize]` attributes on protected endpoints (production-safe).
- [ ] Allow anonymous in tutorial mode via the fallback policy behavior above.

### 5) Testing
- [ ] Unit test: when `AuthEnabled=false`, anonymous requests are allowed.
- [ ] Unit test: when `AuthEnabled=true`, anonymous requests are denied.
 - [ ] Unit test: `ExecutionTiming*` flags toggle headers/logging.
 - [ ] Unit test: `CacheDiagnosticsHeaders` toggles cache headers.

## Risks
- Misconfiguring the flag could open production endpoints. Use environment-specific configuration and CI checks.
 - Conflicting appsettings values can cause confusion; document precedence clearly.
