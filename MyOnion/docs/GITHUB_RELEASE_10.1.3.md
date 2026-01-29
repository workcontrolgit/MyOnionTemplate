# Template OnionAPI v10.1.3 - Feature Management and Value Object Enhancements

## 🎯 What's New

### Feature Management with Microsoft.FeatureManagement

Enable or disable features at runtime without code changes:

- **🔓 Tutorial Mode** - Run the API without JWT configuration (`AuthEnabled: false`)
- **💾 Cache Control** - Toggle caching and diagnostics headers
- **⏱️ Execution Timing** - Control timing headers and logging
- **🗄️ Database Switching** - Choose between SQL Server and in-memory database

Perfect for demos, learning, and environment-specific behavior!

### Value Objects for Domain Modeling

Introduced domain-driven value objects:

- **PersonName** - Encapsulates first/middle/last name with normalization
- **PositionTitle** - Normalized position titles with validation
- **DepartmentName** - Normalized department names with validation

Prevents duplicates like "Sales" vs " sales" and centralizes validation logic.

### Docker Compose Improvements

- Reorganized docker-compose project structure
- Simplified HTTPS certificate mounting
- Updated SQL Server 2022 configuration

## 📦 Installation

**Visual Studio 2022:**
```
Extensions → Manage Extensions → Search "Template OnionAPI"
```

**Or download:** [Visual Studio Marketplace](https://marketplace.visualstudio.com/items?itemName=workcontrol.VSIXTemplateOnionAPI)

## 🚀 Quick Start - Tutorial Mode

Try the template without JWT setup:

1. Create new project from template
2. Set `AuthEnabled: false` in `appsettings.Development.json`:
   ```json
   {
     "FeatureManagement": {
       "AuthEnabled": false
     }
   }
   ```
3. Run and explore at `https://localhost:5001/swagger`

## 📋 Feature Flags

Configure in `appsettings.json`:

```json
{
  "FeatureManagement": {
    "AuthEnabled": true,              // Toggle authentication
    "CacheEnabled": true,              // Master cache switch
    "CacheDiagnosticsHeaders": false,  // Cache debug headers
    "ExecutionTimingEnabled": true,    // Request timing
    "ExecutionTimingIncludeHeader": true,
    "ExecutionTimingIncludePayload": true,
    "ExecutionTimingLogTimings": false,
    "UseInMemoryDatabase": false       // SQL Server vs in-memory
  }
}
```

## 🔧 Technical Highlights

**Authorization Integration:**
```csharp
var enabled = await _featureManager.IsEnabledAsync("AuthEnabled");
if (!enabled)
{
    context.Succeed(requirement); // Tutorial mode - allow all
    return;
}
```

**Cache Control:**
```csharp
var featureEnabled = await _featureManager.IsEnabledAsync("CacheEnabled");
return featureEnabled && options.Enabled && !options.DisableCache;
```

**Value Object Usage:**
```csharp
var employee = new Employee
{
    Name = new PersonName("John", "Q", "Doe")  // Normalized automatically
};
```

## 🔄 Migration from v10.1.2

✅ **No breaking changes** - Backward compatible

1. Add `FeatureManagement` section to appsettings
2. Value objects are internal - no API contract changes
3. Existing code continues to work

## 📦 Dependencies Added

- Microsoft.FeatureManagement (9.0.0)
- Microsoft.FeatureManagement.AspNetCore (9.0.0)

## 🛡️ Security Best Practices

- ✅ Keep `AuthEnabled: true` in production
- ✅ Set `CacheDiagnosticsHeaders: false` in production
- ✅ Use environment variables for sensitive flags
- ✅ Only enable tutorial mode in controlled environments

## 📚 Documentation

- [Feature Management Blog](docs/FeatureManagementBlog.md) - Deep dive into feature flags
- [Release Notes](docs/RELEASE_NOTES_10.1.3.md) - Complete release documentation
- [Value Object Design](docs/ValueObjectDesign.md) - Domain modeling patterns

## 🔗 What's Changed

**Full Changelog:**

- Update build script (60c112f)
- Update docker compose (76553a0)
- Move docker project to MyOnion folder (2219dbb)
- Update template version to 10.1.3 (038f68a)
- Fix feature manager lifetime in request timing middleware (299021a)
- Add feature flags for auth/caching/timing and tighten auth defaults (b5d4fae)
- Add test coverage plans and caching/VO test suites (f3856b6)
- Map value objects as owned types and align query paths (d74d998)
- Add value objects for names and titles (3e1402e)
- Add EasyCaching blog (4d25396)

## 🎓 Use Cases

Perfect for:

- 🎯 Onboarding new developers without JWT complexity
- 🎤 Demos and presentations
- 🧪 Integration testing without auth infrastructure
- 📚 Learning clean architecture patterns
- ⚡ Rapid prototyping

## 📞 Support

- 🐛 **Issues:** [GitHub Issues](https://github.com/workcontrolgit/MyOnionTemplate/issues)
- 💬 **Discussions:** [GitHub Discussions](https://github.com/workcontrolgit/MyOnionTemplate/discussions)
- 📦 **Marketplace:** [Visual Studio Marketplace](https://marketplace.visualstudio.com/items?itemName=workcontrol.VSIXTemplateOnionAPI)

---

**Thank you** to all contributors and community members who provided feedback! 🙏
