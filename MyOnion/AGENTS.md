# Repository Guidelines

For a high-level overview of folders, commands, and onboarding steps, see the root [`README.md`](README.md). This file dives deeper into day-to-day expectations for contributors and automation agents.

## Project Structure & Module Organization
The root `MyOnion.sln` wires the five onion layers: `MyOnion.Domain` holds entities/enums, `MyOnion.Application` contains DTOs, Behaviours, Mapster mappings, and mediator features, `MyOnion.Infrastructure.Persistence` implements EF Core contexts/repositories, `MyOnion.Infrastructure.Shared` exposes reusable services, and `MyOnion.WebApi` hosts controllers, middleware, and `appsettings*.json`. Keep cross-layer references flowing inward (WebApi -> Application -> Domain) and place new assets beside their peers (e.g., `Features/Orders`, `Controllers/OrdersController.cs`).

## Build, Test, and Development Commands
- `dotnet restore MyOnion.sln` downloads NuGet packages for all projects.
- `dotnet build MyOnion.sln -c Release` validates the full solution (net10.0 target) before committing.
- `dotnet run --project MyOnion.WebApi/MyOnion.WebApi.csproj` serves the API; supply `ASPNETCORE_ENVIRONMENT=Development` for local testing.
- `dotnet watch run --project MyOnion.WebApi/MyOnion.WebApi.csproj` hot-reloads while editing controllers, middleware, or configuration.

## Coding Style & Naming Conventions
Follow standard C# conventions: PascalCase for types and public members, camelCase for locals/parameters, and suffix interfaces with `I`. Keep mediator requests/responses in dedicated files under `Features`, and keep Mapster registrations in `Mappings`. Align DTO names with their API contracts. Use expression-bodied members where they improve clarity, prefer dependency injection via constructor parameters, and run `dotnet format` (whitespace + analyzers) before reviews.

## Testing Guidelines
Add xUnit-based projects under a future `tests/` folder (e.g., `tests/MyOnion.Application.Tests`). Name test classes `<FeatureName>Tests` and methods `Should_<Expectation>_When_<Condition>`. Cover validators, repositories, and controller behaviour; target at least the critical paths around paging, filtering, and exception middlewares. Execute `dotnet test MyOnion.sln` locally; wire coverage reports via `--collect:"XPlat Code Coverage"` when high-risk changes ship.

## Commit & Pull Request Guidelines
Existing history uses short imperative messages ("Add project files"), so continue that style: start with a verb, keep under 72 characters, and mention scope when useful (e.g., `Add customer filtering middleware`). PRs should describe the change, list validation commands, reference linked issues, and attach screenshots or sample responses whenever API contracts move. Ensure CI is green before requesting review.

### Check-In Requests
When a user asks to "check in" code, prepare a concise commit message, stage the relevant changes, and perform the commit. Call out any uncommitted unrelated changes before committing and confirm if any files should be excluded.

## Security & Configuration Tips
Never commit real connection strings or secrets; keep shared defaults in `MyOnion.WebApi/appsettings.Development.json` and override locally with `dotnet user-secrets` or environment variables. Configure `Sts:ServerUrl`, `Sts:Audience`, and optional `Sts:ValidIssuer` before running so JWT validation stays strict. Define environment-specific CORS origins under `Cors:AllowedOrigins` to avoid the fallback permissive policy. Infrastructure persistence uses SQL connection resiliency + DbContext pooling, so prefer scoped services and avoid capturing contexts beyond a request. Validate that new settings are documented and loaded via strongly typed options classes in `Infrastructure.Shared` or `WebApi` extensions, and prefer IServiceCollection helpers over manual configuration lookups.
