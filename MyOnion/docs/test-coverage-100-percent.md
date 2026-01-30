# Test Coverage 100% Plan

## Phase 1 – High-Gain Gaps
1. Add unit tests for all validators (Departments, Employees, Positions, SalaryRanges) to cover rule branches.
2. Exercise helper utilities (`ModelHelper`, `DataShapeHelper`) with varied field selections and invalid inputs.
3. Add direct tests for remaining query handlers (`GetDepartmentById`, `GetSalaryRangeById`, etc.) and domain settings / exceptions.
4. Expected outcome: Application + Domain assemblies trend toward ~80% line coverage.

## Phase 2 – Infrastructure Completeness
1. Expand repository tests to cover every branch inside `GenericRepositoryAsync` (paged responses, advanced responses, bulk insert, etc.).
2. Add tests for `DbInitializer` and `ServiceRegistration` to verify both InMemory and SQL configuration paths.
3. Cover Infrastructure.Shared services (`DatabaseSeeder`, `EmailService`, `MockService`, Bogus configs) using fakes/mocks.
4. Expected outcome: Infrastructure assemblies exceed 95% coverage.

## Phase 3 – WebApi & Middleware
1. Add controller tests for edge cases (bad IDs, validation failures, mock insert endpoints).
2. Introduce middleware tests (`ErrorHandlerMiddleware`, `RequestTimingMiddleware`) using `DefaultHttpContext` or TestServer.
3. Exercise `Program`, `AppExtensions`, and `ServiceExtensions` through minimal host builders to hit bootstrapping code.
4. Expected outcome: WebApi assembly surpasses 95% coverage.

## Phase 4 – Polish & Automation
1. Identify any residual uncovered lines via ReportGenerator and write targeted tests or justified exclusions.
2. Integrate `dotnet test --collect:"XPlat Code Coverage"` plus ReportGenerator into CI, gating on 100% line coverage.
3. Document coverage strategy and test ownership to sustain the target over time.
