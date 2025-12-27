# Repository Guidelines

## Project Structure & Module Organization
The root `MyOnion.sln` wires the five onion layers: `MyOnion.Domain` holds entities/enums, `MyOnion.Application` contains DTOs, Behaviours, and MediatR Features, `MyOnion.Infrastructure.Persistence` implements EF Core contexts/repositories, `MyOnion.Infrastructure.Shared` exposes reusable services, and `MyOnion.WebApi` hosts controllers, middleware, and `appsettings*.json`. Keep cross-layer references flowing inward (WebApi -> Application -> Domain) and place new assets beside their peers (e.g., `Features/Orders`, `Controllers/OrdersController.cs`).

## Build, Test, and Development Commands
- `dotnet restore MyOnion.sln` downloads NuGet packages for all projects.
- `dotnet build MyOnion.sln -c Release` validates the full solution (net10.0 target) before committing.
- `dotnet run --project MyOnion.WebApi/MyOnion.WebApi.csproj` serves the API; supply `ASPNETCORE_ENVIRONMENT=Development` for local testing.
- `dotnet watch run --project MyOnion.WebApi/MyOnion.WebApi.csproj` hot-reloads while editing controllers, middleware, or configuration.

## Coding Style & Naming Conventions
Follow standard C# conventions: PascalCase for types and public members, camelCase for locals/parameters, and suffix interfaces with `I`. Keep MediatR requests/responses in dedicated files under `Features`, and align DTO names with their API contracts. Use expression-bodied members where they improve clarity, prefer dependency injection via constructor parameters, and run `dotnet format` (whitespace + analyzers) before reviews.

## Testing Guidelines
Add xUnit-based projects under a future `tests/` folder (e.g., `tests/MyOnion.Application.Tests`). Name test classes `<FeatureName>Tests` and methods `Should_<Expectation>_When_<Condition>`. Cover validators, repositories, and controller behaviour; target at least the critical paths around paging, filtering, and exception middlewares. Execute `dotnet test MyOnion.sln` locally; wire coverage reports via `--collect:"XPlat Code Coverage"` when high-risk changes ship.

## Commit & Pull Request Guidelines
Existing history uses short imperative messages ("Add project files"), so continue that style: start with a verb, keep under 72 characters, and mention scope when useful (e.g., `Add customer filtering middleware`). PRs should describe the change, list validation commands, reference linked issues, and attach screenshots or sample responses whenever API contracts move. Ensure CI is green before requesting review.

## Security & Configuration Tips
Never commit real connection strings or secrets; keep shared defaults in `MyOnion.WebApi/appsettings.Development.json` and override locally with `dotnet user-secrets` or environment variables. Configure `Sts:ServerUrl`, `Sts:Audience`, and optional `Sts:ValidIssuer` before running so JWT validation stays strict. Define environment-specific CORS origins under `Cors:AllowedOrigins` to avoid the fallback permissive policy. Infrastructure persistence uses SQL connection resiliency + DbContext pooling, so prefer scoped services and avoid capturing contexts beyond a request. Validate that new settings are documented and loaded via strongly typed options classes in `Infrastructure.Shared` or `WebApi` extensions, and prefer IServiceCollection helpers over manual configuration lookups.
