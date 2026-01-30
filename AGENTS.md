# Repository Guidelines

## Project Structure & Module Organization
The solution entry point is `MyOnion.sln` at the repository root. Production code lives under `MyOnion/src/` with onion layers: `MyOnion.Domain` (entities/value objects), `MyOnion.Application` (features, DTOs, validators, Mapster mappings), `MyOnion.Infrastructure.Persistence` (EF Core DbContext/repositories/seed data), `MyOnion.Infrastructure.Shared` (cross-cutting services), and `MyOnion.WebApi` (controllers, middleware, Swagger, configuration). Tests belong under `MyOnion/tests/`. Documentation and automation live in `MyOnion/docs/` and `MyOnion/scripts/`, with build artifacts in `MyOnion/artifacts/` and VSIX packaging in `MyOnion/vsix/`.

## Build, Test, and Development Commands
- `dotnet restore MyOnion.sln` restores NuGet packages for the full solution.
- `dotnet build MyOnion.sln -c Release` validates the solution before PRs.
- `dotnet run --project MyOnion.WebApi/MyOnion.WebApi.csproj` runs the API locally.
- `dotnet watch run --project MyOnion.WebApi/MyOnion.WebApi.csproj` runs with hot reload.
- `docker compose up --build` runs the API + SQL Server (copy `MyOnion/.env.example` to `MyOnion/.env` first).

## Coding Style & Naming Conventions
Follow standard C# conventions: PascalCase for types/public members, camelCase for locals/parameters, and `I`-prefixed interfaces. Place new features under `MyOnion.Application/Features/<FeatureName>` with request/handler pairs and validators; keep Mapster mappings in `Mappings`. Use constructor injection and keep dependencies flowing inward (WebApi -> Application -> Domain). Run `dotnet format` before review to apply whitespace and analyzer rules.

## Testing Guidelines
Automated tests are not yet committed; add xUnit projects under `MyOnion/tests/` (for example, `MyOnion.Application.Tests`). Name test classes `<FeatureName>Tests` and methods `Should_<Expectation>_When_<Condition>`. Run `dotnet test MyOnion.sln`; collect coverage with `--collect:"XPlat Code Coverage"` for high-risk changes.

## Commit & Pull Request Guidelines
Recent history uses short imperative subjects (for example, "Update template version to 10.2.0", "Migrate to Ardalis.Specification library"). Follow that pattern: start with a verb, keep the subject under 72 characters, and mention scope if needed. PRs should include a summary, validation commands, linked issues, and screenshots or sample responses when API contracts change.

## Security & Configuration Tips
Do not commit secrets. Store defaults in `MyOnion.WebApi/appsettings.Development.json`, then override with `dotnet user-secrets` or environment variables. Configure `Sts:ServerUrl`, `Sts:Audience`, and optional `Sts:ValidIssuer` before running. Lock down CORS using `Cors:AllowedOrigins` arrays for non-local environments.
