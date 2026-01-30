# Template Onion API on .NET 10: Ready for Visual Studio 2026

**TL;DR**  
Template Onion API’s Net 10 refresh delivers spec-driven repositories, a Result-based pipeline, NSwag-powered Swagger, endpoint timing telemetry, and full test scaffolding. It installs cleanly on Visual Studio 2026 (and 2022) so you can scaffold production-ready clean-architecture APIs in minutes. The template has been downloaded 4,428 times since its 2021 launch—grab it now: https://marketplace.visualstudio.com/items?itemName=workcontrol.VSIXTemplateOnionAPI  
Full source code + issue tracker: https://github.com/workcontrolgit/MyOnionTemplate

---

## Why This Upgrade Matters

Visual Studio 2026 just launched, and the first question most .NET developers ask is: *“Will my templates keep up?”* Template Onion API does more than keep up—it takes full advantage of .NET 10 and the new IDE, shipping a battle-tested clean-architecture stack with modern tooling baked in.

- **.NET 10 + C# 13**: Every layer (Domain, Application, Infrastructure, WebApi) and every test project target `net10.0`, so you get the latest language features plus a single SDK to maintain.
- **Security & Performance Guardrails**: JWT authentication enforces HTTPS-only tokens, CORS policies read from configuration, and DbContext pooling with retry-on-failure guards against transient SQL hiccups.
- **Specification-Driven Repositories**: Shared `ISpecification<T>` contracts keep filtering, paging, and eager-loading logic in one place. Repositories simply compose specs, reducing duplicate LINQ and making new features faster to build.
- **Result Pattern + Data Shaping Boosts**: The new `Result`, `Result<T>`, and paged variants standardize JSON responses while DataShapeHelper caching trims reflection overhead. Every response includes execution-time metadata for instant observability.
- **Swagger Without Swashbuckle**: NSwag replaces Swashbuckle, ensuring Swagger UI and OpenAPI JSON stay compatible with .NET 10. JWT security schemes, versioning, and camelCase serialization are wired up out of the box.
- **Test Suites Included**: Three xUnit projects cover application handlers, repositories, and controllers, aligned with coverage/ReportGenerator guidance. `dotnet test --collect:"XPlat Code Coverage"` is ready the moment you scaffold.

---

## Feature Spotlight: Specification Pattern + Result Pipeline

Two pillars define this release:

1. **Specification Pattern Everywhere**  
   - Centralized filters, includes, and ordering keep repositories consistent.  
   - Specs plug directly into EF Core through a shared evaluator, so paging and filtering stay expressive without scattering LINQ.

2. **Result-Based API Contracts with Timing**  
   - `Result`, `Result<T>`, and `PagedResult` standardize success/failure metadata.  
   - The execution-time filter injects `x-execution-time-ms` headers and populates Result payloads, giving ops teams instant latency metrics.

Together, they produce predictable JSON, faster responses, and better diagnostics—exactly what enterprise APIs demand.

---

## Getting Started (Visual Studio 2026 or 2022)

1. **Install the VSIX**  
   Download Template Onion API from the Marketplace: https://marketplace.visualstudio.com/items?itemName=workcontrol.VSIXTemplateOnionAPI.

2. **Create Your Solution**  
   In Visual Studio 2026 (or 2022), choose “Create a new project,” search for “Template OnionAPI (.NET 10),” and select it.

3. **Name It Once, Everywhere**  
   Enter your project name (e.g., `ContosoCore`). The wizard stamps that name across every layer: `ContosoCore.WebApi`, `ContosoCore.Application`, `ContosoCore.Infrastructure`, plus all three test projects.

4. **Run from Visual Studio**  
   Set `<ProjectName>.WebApi` as the startup project, press F5 (or Ctrl+F5). Visual Studio 2026’s debugger launches your API instantly.

5. **Inspect Swagger + Timing**  
   Browse to `/swagger` to see the NSwag UI with JWT auth enabled. Check the response headers for `x-execution-time-ms` to confirm endpoint timing telemetry.

6. **Validate with Tests**  
   Open a terminal at the solution root and run  
   ```bash
   dotnet test <ProjectName>.sln --collect:"XPlat Code Coverage"
   ```  
   You’ll exercise application, infrastructure, and Web API suites in one shot.

---

## Where To Go Next

- **Extend Features**: Add new specs under `src/<ProjectName>.Application/Specifications/<Feature>` and handlers under `Features/<FeatureName>`.
- **Automate Builds**: Wire up CI to run `dotnet build -c Release` and `dotnet test` with coverage.
- **Ship Faster**: Rebuild the VSIX using `scripts/Build-OnionTemplate.ps1` whenever you customize the template, ensuring teammates get your improvements instantly.

Visual Studio 2026 may be new, but you don’t have to wait for your architecture foundation to catch up. Template Onion API on .NET 10 is already there—download it, scaffold your next API, and hit the ground running.
