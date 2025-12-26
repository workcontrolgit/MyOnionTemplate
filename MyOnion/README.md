# MyOnion

MyOnion is a .NET 9 onion-architecture reference API that demonstrates Domain-Driven Design patterns using ASP.NET Core, MediatR, FluentValidation, and EF Core. The solution is organized into Domain, Application, Infrastructure.Persistence, Infrastructure.Shared, and WebApi projects, keeping dependencies flowing inward so UI concerns never reference infrastructure details directly.

## Project Layout
- `MyOnion.Domain` – Entities, enums, shared value objects, and domain abstractions.
- `MyOnion.Application` – DTOs, Behaviours, MediatR request/handler pipelines, validators, and service interfaces.
- `MyOnion.Infrastructure.Persistence` – EF Core DbContext, repositories, and seed data used to back the application layer.
- `MyOnion.Infrastructure.Shared` – Cross-cutting services (e.g., external integrations, mocks) registered for DI.
- `MyOnion.WebApi` – ASP.NET Core host exposing controllers, middleware, Swagger UI, and configuration.

## Getting Started
```powershell
# Install required SDK
winget install Microsoft.DotNet.SDK.9

# Restore dependencies
dotnet restore MyOnion.sln

# Build (Release configuration recommended before PRs)
dotnet build MyOnion.sln -c Release

# Run the API with hot reload
dotnet watch run --project MyOnion.WebApi/MyOnion.WebApi.csproj
```
Navigate to `https://localhost:5001/swagger` for API exploration. Health checks are exposed at `/health`.

## Coding Standards
- Follow standard C# conventions (PascalCase for types, camelCase for locals/parameters, interfaces prefixed with `I`).
- Place new features under `MyOnion.Application/Features/<FeatureName>` with paired request/response handlers.
- Keep DTO names synchronized with their controller endpoints, and prefer constructor injection.
- Run `dotnet format` before submitting changes to enforce style and analyzer rules.

## Testing
Automated tests are not yet included; add future suites under `tests/` (xUnit recommended) covering validators, repositories, and controllers. Execute with `dotnet test MyOnion.sln` and collect coverage using `--collect:"XPlat Code Coverage"` for critical features.

## Security & Configuration Notes
- JWT authentication is configured via `AddJWTAuthentication`; provide `Sts:ServerUrl` and `Sts:Audience` via `appsettings.Development.json` or user secrets.
- Do not commit secrets. Use `dotnet user-secrets` locally or environment variables in CI/CD.
- Default CORS policy currently allows all origins; restrict this in production using configuration-driven policies.

## Contributing
1. Fork or branch from `main`.
2. Add or update code alongside matching layer projects.
3. Validate with `dotnet build` and manual API smoke tests.
4. Submit a PR using imperative commit messages (e.g., `Add customer filtering`). Include a short description, validation commands, and screenshots or sample responses if your change alters API contracts.
