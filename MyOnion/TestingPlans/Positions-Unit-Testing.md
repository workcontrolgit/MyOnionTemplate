# Position Feature Testing Plan

Goal: introduce automated tests covering the Positions feature across application, infrastructure, and API layers.

## Scope
1. **Domain/Application layer**
   - Validate `CreatePositionCommandHandler`, `UpdatePositionCommandHandler`, and `DeletePositionByIdCommandHandler`.
   - Verify `GetPositionsQueryHandler` filtering/field validation logic and `GetPositionByIdQueryHandler`.
   - Exercise `PositionRepositoryAsync` mock interactions and AutoMapper profiles.
2. **Infrastructure layer**
   - Cover `PositionRepositoryAsync` using an EF Core in-memory context (read/write, data shaping).
   - Seed/mock data validation (bulk insert + spec evaluation).
3. **Web API layer**
   - Controller action tests using `WebApplicationFactory` / `TestServer` to ensure endpoints accept/return expected contracts (DTOs, status codes, validation errors).

## Tasks
1. **Test project setup**
   - Add `tests/MyOnion.Application.Tests`, `tests/MyOnion.Infrastructure.Tests`, and `tests/MyOnion.WebApi.Tests` projects targeting `net10.0`.
   - Reference corresponding production projects and common test packages (xUnit, FluentAssertions, Moq, Microsoft.AspNetCore.Mvc.Testing).
2. **Application tests**
   - Mock `IPositionRepositoryAsync`, `IMapper`, and `IModelHelper` to isolate handlers.
   - Write test cases for success/failure paths (e.g., duplicate position number, validation behavior).
3. **Infrastructure tests**
   - Spin up `ApplicationDbContext` with `UseInMemoryDatabase`.
   - Test repository methods (`IsUniquePositionNumberAsync`, `GetPositionReponseAsync`) for correct data shaping and record counts.
4. **Web API tests**
   - Use `WebApplicationFactory<MyOnion.WebApi.Program>` to send HTTP requests to `/api/v1/positions`.
   - Assert payloads map to `PositionSummaryDto`, authorization/validation responses, and middleware behaviors (e.g., execution timing header if feasible).
5. **CI integration**
   - Update solution to include new test projects.
   - Add `dotnet test` invocation to build scripts/pipelines.

## Deliverables
- Three test projects with comprehensive coverage of Position feature.
- Documentation (README/TestPlan) describing how to run tests and expected coverage.
