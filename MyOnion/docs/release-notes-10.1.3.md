# Template OnionAPI v10.1.3 Release Notes

**Release Date:** January 2026
**Download:** [Visual Studio Marketplace](https://marketplace.visualstudio.com/items?itemName=workcontrol.VSIXTemplateOnionAPI)

## Overview

Version 10.1.3 delivers Feature Management integration, Value Object enhancements, and Docker Compose improvements to Template OnionAPI. This release enables flexible environment configuration through Microsoft.FeatureManagement, strengthens domain modeling with value objects, and simplifies containerized development.

## 🎯 Key Features

### Feature Management with Microsoft.FeatureManagement

Template OnionAPI now supports runtime feature toggles for authentication, caching, and diagnostics without code changes or redeployment.

**Implemented Feature Flags:**

- **AuthEnabled** - Toggle authentication enforcement (enables tutorial mode when `false`)
- **CacheEnabled** - Master switch for EasyCaching functionality
- **CacheDiagnosticsHeaders** - Control cache diagnostic headers (`X-Cache-Status`, `X-Cache-Key`, etc.)
- **ExecutionTimingEnabled** - Toggle request timing middleware
- **ExecutionTimingIncludeHeader** - Control `x-execution-time-ms` response header
- **ExecutionTimingIncludePayload** - Include execution time in result payload
- **ExecutionTimingLogTimings** - Toggle execution timing log entries
- **UseInMemoryDatabase** - Switch between SQL Server and in-memory database (startup-only)

**Tutorial Mode:**

Developers can now run the template without JWT configuration by setting `AuthEnabled: false` in `appsettings.Development.json`. This is perfect for demos, learning, and rapid prototyping.

```json
{
  "FeatureManagement": {
    "AuthEnabled": false
  }
}
```

**Read the full blog:** [Feature Management in Template OnionAPI v10.1.3](FeatureManagementBlog.md)

### Value Object Domain Modeling

Introduced domain-driven value objects for better data consistency and validation:

**Value Objects:**

- **PersonName** - Encapsulates `FirstName`, `MiddleName`, `LastName` with normalization and `FullName` formatting
- **PositionTitle** - Normalized position titles with validation and equality comparison
- **DepartmentName** - Normalized department names with validation and equality comparison

**Benefits:**

- Centralized normalization and validation logic
- Prevents duplicate variants (e.g., "Sales" vs " sales" vs "Sales ")
- Consistent display formatting across APIs and reports
- Type-safe domain modeling with value equality semantics

**Implementation:**

Value objects are mapped as owned types in EF Core, storing as columns in parent tables while maintaining domain encapsulation:

```csharp
public sealed class PersonName
{
    public string FirstName { get; }
    public string MiddleName { get; }
    public string LastName { get; }
    public string FullName => $"{FirstName} {MiddleName} {LastName}".Trim();

    private PersonName() { } // EF Core
    public PersonName(string firstName, string middleName, string lastName)
    {
        FirstName = Normalize(firstName);
        MiddleName = Normalize(middleName);
        LastName = Normalize(lastName);
    }
}
```

**Read the design plan:** [Value Object Design](ValueObjectDesign.md)

### Docker Compose Enhancements

**Reorganized Docker Project:**

- Moved docker-compose project into `MyOnion/` folder for better organization
- Updated docker-compose configuration for improved container networking
- Simplified HTTPS certificate mounting for local development

**Docker Compose Services:**

```yaml
services:
  myonion-webapi:
    container_name: MyOnion.WebApi
    ports:
      - "8080:8080"   # HTTP
      - "44378:8443"  # HTTPS
    depends_on:
      - myonion-sql

  myonion-sql:
    image: mcr.microsoft.com/mssql/server:2022-latest
    ports:
      - "14333:1433"
```

**Quick Start:**

```powershell
# Create dev certificate
./MyOnion/scripts/Create-DevCert.ps1 -Password "devpassword"

# Start services
docker compose --project-directory MyOnion up --build
```

### Build Script Updates

- Simplified VSIX template output path from `ProjectTemplates\CSharp\1033` to `ProjectTemplates`
- Improved template packaging process
- Enhanced error handling in build automation

## 🔧 Technical Improvements

### Authorization Integration

Feature management integrates seamlessly with ASP.NET Core authorization:

```csharp
public sealed class AuthEnabledRequirementHandler : AuthorizationHandler<AuthEnabledRequirement>
{
    protected override async Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        AuthEnabledRequirement requirement)
    {
        var enabled = await _featureManager.IsEnabledAsync("AuthEnabled");
        if (!enabled)
        {
            context.Succeed(requirement); // Tutorial mode
            return;
        }

        if (context.User?.Identity?.IsAuthenticated == true)
        {
            context.Succeed(requirement); // Authenticated
        }
    }
}
```

### Cache Control with Feature Flags

EasyCaching respects the feature flag hierarchy:

```csharp
private bool IsCacheEnabled(CachingOptions options)
{
    var featureEnabled = _featureManager.IsEnabledAsync("CacheEnabled").GetAwaiter().GetResult();
    return featureEnabled && options.Enabled && !options.DisableCache && !_bypassContext.ShouldBypass;
}
```

### Database Provider Selection

Database provider is chosen at startup based on feature configuration:

```csharp
var featureUseInMemory = configuration.GetSection("FeatureManagement").GetValue<bool?>("UseInMemoryDatabase");
var useInMemory = featureUseInMemory ?? configuration.GetValue<bool>("UseInMemoryDatabase");
if (useInMemory)
{
    services.AddDbContext<ApplicationDbContext>(options =>
        options.UseInMemoryDatabase("ApplicationDb"));
}
else
{
    services.AddDbContextPool<ApplicationDbContext>(options =>
        options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));
}
```

## 📦 NuGet Package Additions

- **Microsoft.FeatureManagement** (9.0.0) - Feature flag management
- **Microsoft.FeatureManagement.AspNetCore** (9.0.0) - ASP.NET Core integration

## 🔄 Migration Guide

### From v10.1.2 to v10.1.3

1. **Add FeatureManagement configuration** to `appsettings.json`:

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

2. **Update entity references** if using custom entities:

   - `Employee` now uses `PersonName` value object
   - `Position` now uses `PositionTitle` value object
   - `Department` now uses `DepartmentName` value object

3. **No API contract changes** - DTOs remain string-based, value objects are internal domain concern

## ⚠️ Breaking Changes

**None** - This release is backward compatible with v10.1.2. Existing APIs continue to work without modification.

## 🛡️ Security Considerations

1. **AuthEnabled Default:** Keep `AuthEnabled: true` in production environments
2. **Tutorial Mode:** Only set `AuthEnabled: false` in controlled demo/learning environments
3. **Diagnostics Headers:** Keep `CacheDiagnosticsHeaders: false` in production to prevent cache key exposure
4. **Environment Variables:** Override sensitive flags using environment variables rather than committing them

## 📊 Configuration Precedence

Feature flags follow this precedence (highest to lowest):

1. Environment variables (e.g., `FeatureManagement__AuthEnabled=false`)
2. User secrets (development)
3. `appsettings.{Environment}.json`
4. `appsettings.json`

## 🎓 Tutorial Mode Use Cases

Perfect for:

- **Onboarding new developers** - Explore the API immediately without JWT setup
- **Demos and presentations** - Show functionality without authentication complexity
- **Integration testing** - Test endpoints without auth infrastructure
- **Learning clean architecture** - Focus on domain logic, not auth plumbing
- **Rapid prototyping** - Build features first, add auth later

## 📝 Complete Commit History

```
d059221 Merge branch 'release/10.1.3'
60c112f Update build script
5972079 Merge branch 'feature/Docker-Compose' into develop
76553a0 Update docker compose
2219dbb Move docker project to MyOnion folder
038f68a Update template version to 10.1.3
1c8c342 Merge branch 'feature/Feature-Mangement' into develop
299021a Fix feature manager lifetime in request timing middleware
b5d4fae Add feature flags for auth/caching/timing and tighten auth defaults
f3856b6 Add test coverage plans and caching/VO test suites
675717b Merge branch 'feature/Value-Object' into develop
d74d998 Map value objects as owned types and align query paths
3e1402e Add value objects for names and titles
4d25396 Add EasyCaching blog
```

## 🔗 Related Documentation

- [Feature Management Blog](FeatureManagementBlog.md) - Deep dive into feature flags
- [Value Object Design](ValueObjectDesign.md) - Domain modeling patterns
- [EasyCaching Blog](EasyCachingBlog.md) - Caching implementation
- [Docker Support Plan](Docker%20Support%20Plan.md) - Container strategy

## 🙏 Acknowledgments

This release incorporates feedback from the community on flexible authentication, cleaner domain modeling, and improved development experience. Thank you to all contributors!

## 📞 Support

- **Issues:** [GitHub Issues](https://github.com/workcontrolgit/MyOnionTemplate/issues)
- **Marketplace:** [Visual Studio Marketplace](https://marketplace.visualstudio.com/items?itemName=workcontrol.VSIXTemplateOnionAPI)
- **Documentation:** [README.md](../README.md)

---

**Next Steps:**

1. Download the updated template from Visual Studio Marketplace
2. Create a new project using the template
3. Explore tutorial mode by setting `AuthEnabled: false`
4. Review the feature management blog for advanced configuration
5. Join the community and share your feedback!
