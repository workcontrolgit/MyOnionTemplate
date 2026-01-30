# Unit Tests Enhancement Plan

## Goals
- Increase coverage for value objects, cache behaviors, and EF translation paths.
- Protect recent enhancements from regression.
- Document coverage targets and required test layers.

## Scope
- Domain value objects (DepartmentName, PositionTitle, PersonName).
- Application behaviors (caching, spec order-by mapping).
- Infrastructure EF translation (owned type queries).
- Web API cache diagnostics and bypass middleware.

## Current Test Inventory (as of this plan)
- Domain: command handler tests exist; value object tests were added.
- Application: query handlers covered; caching behaviors now covered.
- Infrastructure: repository CRUD tests exist; SQLite translation tests added.
- Web API: controller tests exist; cache bypass and diagnostics tests added.

## Coverage Targets
- Domain: >= 90% for value objects.
- Application: >= 80% for caching behaviors and specs.
- Infrastructure: >= 70% for repository query paths.
- Web API: >= 60% for cache headers/bypass and dashboard metrics.

## Task Checklist

### 1) Domain Value Objects
- [ ] DepartmentName: normalize, empty/whitespace, max length.
- [ ] DepartmentName: equality case-insensitive.
- [ ] PositionTitle: normalize, empty/whitespace, max length.
- [ ] PositionTitle: equality case-insensitive.
- [ ] PersonName: required first/last, max length per part.
- [ ] PersonName: full name formatting (with/without middle name).

### 2) Application Specs + Mapping
- [ ] DepartmentsByFiltersSpecification: Name filter uses Name.Value.
- [ ] DepartmentsByFiltersSpecification: OrderBy maps "Name" -> "Name.Value".
- [ ] PositionsByFiltersSpecification: PositionTitle filter uses PositionTitle.Value.
- [ ] PositionsByFiltersSpecification: Department filter uses Department.Name.Value.
- [ ] PositionsByFiltersSpecification: OrderBy maps PositionTitle/Department to Value paths.
- [ ] EmployeesByFiltersSpecification: PositionTitle filter uses PositionTitle.Value.

### 3) Cache Behaviors
- [ ] GetEmployeesCachingDecorator: cache hit returns cached payload without handler call.
- [ ] GetEmployeesCachingDecorator: cache miss caches payload and publishes diagnostics.
- [ ] GetEmployeesCachingDecorator: invalid payload skips caching.
- [ ] GetPositionsCachingBehavior: cache hit path.
- [ ] GetPositionsCachingBehavior: cache miss path.
- [ ] Cache key builders: filter normalization + paging in key.

### 4) Infrastructure EF Translation
- [ ] DepartmentRepositoryAsync: Name filter with owned Name.Value (SQLite in-memory).
- [ ] PositionRepositoryAsync: PositionTitle and Department filters (SQLite in-memory).
- [ ] DashboardMetricsReader: GroupBy projections on Name.Value and PositionTitle.Value.
- [ ] Dynamic OrderBy: "Name.Value" and "PositionTitle.Value" paths.

### 5) Web API Coverage
- [ ] CacheBypassMiddleware: honors X-Debug-Disable-Cache header.
- [ ] Cache diagnostics headers: X-Cache-Status, X-Cache-Key, X-Cache-Duration-Ms.
- [ ] Dashboard metrics endpoint returns expected shape and counts.

## Notes
- Prefer SQLite in-memory for translation tests; EF InMemory will not surface SQL translation issues.
- Keep domain tests pure (no EF) for fast feedback.
- Add tests incrementally to avoid large diffs and easier review.

## SQLite Setup (Sample)
Add packages to the test project:
- Microsoft.EntityFrameworkCore.Sqlite
- Microsoft.Data.Sqlite

Minimal fixture for EF translation tests:
```csharp
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

public sealed class SqliteDbFixture : IDisposable
{
    public SqliteConnection Connection { get; }
    public DbContextOptions<ApplicationDbContext> Options { get; }

    public SqliteDbFixture()
    {
        Connection = new SqliteConnection("DataSource=:memory:");
        Connection.Open();

        Options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseSqlite(Connection)
            .Options;

        using var context = new ApplicationDbContext(Options, new DateTimeService(), LoggerFactory.Create(_ => { }));
        context.Database.EnsureCreated();
    }

    public void Dispose() => Connection.Dispose();
}
```

Usage in a test:
```csharp
public class DepartmentRepositoryAsyncTests : IClassFixture<SqliteDbFixture>
{
    private readonly ApplicationDbContext _context;

    public DepartmentRepositoryAsyncTests(SqliteDbFixture fixture)
    {
        _context = new ApplicationDbContext(fixture.Options, new DateTimeService(), LoggerFactory.Create(_ => { }));
    }

    [Fact]
    public async Task FiltersByName_ValueObject()
    {
        _context.Departments.Add(new Department { Id = Guid.NewGuid(), Name = new DepartmentName("Operations") });
        await _context.SaveChangesAsync();

        // Run repo/spec query; SQLite will validate translation.
    }
}
```
