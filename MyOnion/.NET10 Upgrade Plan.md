# .NET 10 Upgrade Plan

## Objectives
- Move all projects in `MyOnion.sln` from `net9.0` to `net10.0` while preserving clean architecture layering.
- Modernize dependencies (AutoMapper, FluentValidation, MediatR, EF Core) to versions that explicitly support .NET 10.
- Incorporate security hardening (HTTPS-only JWT, locked-down CORS) and performance improvements (DbContext pooling, connection resiliency) during the upgrade.

## Status
- [x] All projects now target `net10.0` (LangVersion `preview`) and build with the .NET 10 SDK locally.
- [x] Security/performance guardrails delivered: JWT auth enforces HTTPS + token validation parameters, CORS reads `Cors:AllowedOrigins`, and SQL contexts use pooling with transient-fault retries.
- [ ] Remaining future work: dependency bumps and extended test suites once .NET 10-compatible NuGet updates ship.

## Phases & Tasks
### 1. Planning & Environment Readiness (Week 1)
- Draft ADR capturing scope, risks, and rollback plan; open a dedicated `feature/net10-upgrade` branch.
- Ensure local/CI agents install the .NET 10 SDK; update `global.json` or pipeline tooling images accordingly.
- Capture current benchmarks (`dotnet --info`, `dotnet test MyOnion.sln --collect:"XPlat Code Coverage"`, API latency snapshots) for later comparison.

### 2. SDK & Project Updates (Week 2)
- Update every `.csproj` `<TargetFramework>` to `net10.0` and set `LangVersion` to `preview` until GA.
- Re-run `dotnet restore` to confirm targeting packs download cleanly; fix warnings flagged by new analyzers.
- Refresh tooling (`dotnet tool restore`, `dotnet format`) so linting runs under the new SDK.

### 3. Dependency & Code Adjustments (Weeks 2-3)
- Upgrade NuGet packages to .NET 10-compatible releases (AutoMapper 13.x+, FluentValidation 12.x, MediatR 13.x, EF Core 10.x, Serilog sinks).
- Address API changes (EF Core query updates, ASP.NET middleware adjustments) and re-run solution-wide builds.
- Apply security fixes: enforce `RequireHttpsMetadata=true`, configure `TokenValidationParameters`, and restrict CORS origins per environment via configuration files.
- Apply performance fixes: switch to `AddDbContextPool`, enable `EnableRetryOnFailure`, and review transient service lifetimes for repositories/clients.

### 4. Testing & Validation (Week 4)
- Execute `dotnet test MyOnion.sln` plus any integration suites against SQL Server.
- Run `dotnet watch run` sanity checks, manual smoke tests via Swagger, and health-check endpoints.
- Perform load testing on critical APIs to compare latency/throughput with pre-upgrade baselines; inspect Serilog metrics for regressions.

### 5. Documentation, Release Prep, and Deployment (Week 5)
- Update AGENTS.md/README with new prerequisites, commands, and security/performance notes.
- Adjust CI/CD YAML to target .NET 10 and ensure secrets handling (user-secrets, Key Vault, etc.) remains intact.
- Stage release notes detailing security hardening (JWT, CORS), performance boosts, and any migration steps for consumers.
- Final sign-off after QA + security review; deploy to staging, monitor health, then roll out to production with rollback plan ready.
