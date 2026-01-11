# Position Feature Testing Plan

Goal: provide automated coverage for the Positions feature across application, infrastructure, and API layers.

## Scope
1. **Application layer**
   - Command handlers: create, update, delete (happy path + error conditions).
   - Query handlers: get-by-id and list (filtering, field validation, error paths).
   - Mapster configuration validation and ValidationBehavior interactions.
2. **Infrastructure layer**
   - `PositionRepositoryAsync` read/write operations using EF Core InMemory provider.
   - Data shaping utilities and specification filters.
3. **Web API layer**
   - Controller endpoints via `WebApplicationFactory` to verify payloads, status codes, and middleware (e.g., execution timing header).

## Tasks
1. **Add test projects**
   - `tests/MyOnion.Application.Tests`, `tests/MyOnion.Infrastructure.Tests`, `tests/MyOnion.WebApi.Tests` targeting `net10.0`.
   - Reference production projects and install xUnit, FluentAssertions, Moq, Microsoft.AspNetCore.Mvc.Testing as needed.
2. **Application tests**
   - Mock repositories/mappers and assert handler outputs, exceptions, and validation.
   - Cover `GetPositionsQueryHandler` field filtering + paging logic.
3. **Infrastructure tests**
   - Seed real entities via InMemory context and evaluate repository/specification behavior.
4. **Web API tests**
   - Use `WebApplicationFactory` to issue HTTP requests against `/api/v1/positions`.
   - Assert DTO shapes (`PositionSummaryDto`), validation responses, and headers.
5. **CI integration**
   - Include new test projects in solution.
   - Update build pipeline to run `dotnet test` across all test projects.

## Deliverables
- Documented testing approach (this file) linked in Solution Items.
- Test projects with suites covering the above scope.
- Updated CI configuration to execute the tests.
