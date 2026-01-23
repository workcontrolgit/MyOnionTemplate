# Dashboard API Plan - MyOnion

## Project Overview
Create a dedicated Dashboard API endpoint in the MyOnion backend that aggregates HR metrics and returns them in a single optimized response for the Angular frontend.

## Goals
- Reduce frontend round-trips by returning a single dashboard payload.
- Keep metrics queries efficient and read-only.
- Support cache + invalidation when employee/position/department data changes.
- Align with MyOnion clean architecture and existing MediatR patterns.

## Architecture Overview (MyOnion)
- **WebApi**: `DashboardController` (v1) + route wiring.
- **Application**: query/handler + DTOs in `Features/Dashboard/Queries`.
- **Infrastructure.Persistence**: query helpers that use EF Core directly or reuse existing repos.

## API Endpoint Specification
- **URL:** `GET /api/v1/dashboard/metrics`
- **Auth:** Required (Bearer token).
- **Roles:** All authenticated users; optional role-based filtering can be added later.
- **Status Codes:** 200, 401, 500.

## DTOs and Models (Application Layer)
**File:** `MyOnion/src/MyOnion.Application/Features/Dashboard/Queries/GetDashboardMetrics/DashboardMetricsDto.cs`

```csharp
namespace MyOnion.Application.Features.Dashboard.Queries.GetDashboardMetrics;

public sealed class DashboardMetricsDto
{
    public int TotalEmployees { get; set; }
    public int TotalDepartments { get; set; }
    public int TotalPositions { get; set; }
    public int TotalSalaryRanges { get; set; }
    public int NewHiresThisMonth { get; set; }
    public decimal AverageSalary { get; set; }
    public List<DepartmentMetricDto> EmployeesByDepartment { get; set; } = new();
    public List<PositionMetricDto> EmployeesByPosition { get; set; } = new();
    public List<SalaryRangeMetricDto> EmployeesBySalaryRange { get; set; } = new();
    public GenderMetricDto GenderDistribution { get; set; } = new();
    public List<RecentEmployeeDto> RecentEmployees { get; set; } = new();
}

public sealed class DepartmentMetricDto
{
    public Guid DepartmentId { get; set; }
    public string DepartmentName { get; set; } = string.Empty;
    public int EmployeeCount { get; set; }
}

public sealed class PositionMetricDto
{
    public Guid PositionId { get; set; }
    public string PositionTitle { get; set; } = string.Empty;
    public int EmployeeCount { get; set; }
}

public sealed class SalaryRangeMetricDto
{
    public Guid SalaryRangeId { get; set; }
    public string RangeName { get; set; } = string.Empty;
    public decimal MinSalary { get; set; }
    public decimal MaxSalary { get; set; }
    public int EmployeeCount { get; set; }
}

public sealed class GenderMetricDto
{
    public int Male { get; set; }
    public int Female { get; set; }
}

public sealed class RecentEmployeeDto
{
    public Guid Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string PositionTitle { get; set; } = string.Empty;
    public string DepartmentName { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}
```

## Application Query + Handler
**File:** `MyOnion/src/MyOnion.Application/Features/Dashboard/Queries/GetDashboardMetrics/GetDashboardMetricsQuery.cs`

```csharp
namespace MyOnion.Application.Features.Dashboard.Queries.GetDashboardMetrics;

public sealed class GetDashboardMetricsQuery : IRequest<Result<DashboardMetricsDto>>
{
}

public sealed class GetDashboardMetricsQueryHandler
    : IRequestHandler<GetDashboardMetricsQuery, Result<DashboardMetricsDto>>
{
    private readonly IDashboardMetricsReader _reader;

    public GetDashboardMetricsQueryHandler(IDashboardMetricsReader reader)
    {
        _reader = reader;
    }

    public async Task<Result<DashboardMetricsDto>> Handle(
        GetDashboardMetricsQuery request,
        CancellationToken cancellationToken)
    {
        var dto = await _reader.GetDashboardMetricsAsync(cancellationToken);
        return Result<DashboardMetricsDto>.Success(dto);
    }
}
```

## Persistence Reader (Infrastructure.Persistence)
**File:** `MyOnion/src/MyOnion.Infrastructure.Persistence/Readers/DashboardMetricsReader.cs`

```csharp
public interface IDashboardMetricsReader
{
    Task<DashboardMetricsDto> GetDashboardMetricsAsync(CancellationToken ct);
}

public sealed class DashboardMetricsReader : IDashboardMetricsReader
{
    private readonly ApplicationDbContext _context;

    public DashboardMetricsReader(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<DashboardMetricsDto> GetDashboardMetricsAsync(CancellationToken ct)
    {
        var now = DateTime.UtcNow;
        var startOfMonth = new DateTime(now.Year, now.Month, 1);

        var totalEmployeesTask = _context.Employees.CountAsync(ct);
        var totalDepartmentsTask = _context.Departments.CountAsync(ct);
        var totalPositionsTask = _context.Positions.CountAsync(ct);
        var totalSalaryRangesTask = _context.SalaryRanges.CountAsync(ct);
        var newHiresTask = _context.Employees.Where(e => e.CreatedAt >= startOfMonth).CountAsync(ct);
        var averageSalaryTask = _context.Employees.AverageAsync(e => (decimal?)e.Salary, ct);

        var deptTask = _context.Employees
            .AsNoTracking()
            .GroupBy(e => new { e.DepartmentId, e.Department.Name })
            .Select(g => new DepartmentMetricDto
            {
                DepartmentId = g.Key.DepartmentId,
                DepartmentName = g.Key.Name,
                EmployeeCount = g.Count()
            })
            .OrderByDescending(d => d.EmployeeCount)
            .ToListAsync(ct);

        var positionTask = _context.Employees
            .AsNoTracking()
            .GroupBy(e => new { e.PositionId, e.Position.PositionTitle })
            .Select(g => new PositionMetricDto
            {
                PositionId = g.Key.PositionId,
                PositionTitle = g.Key.PositionTitle,
                EmployeeCount = g.Count()
            })
            .OrderByDescending(p => p.EmployeeCount)
            .Take(10)
            .ToListAsync(ct);

        var salaryTask = _context.Employees
            .AsNoTracking()
            .GroupBy(e => new
            {
                e.Position.SalaryRangeId,
                e.Position.SalaryRange.Name,
                e.Position.SalaryRange.MinSalary,
                e.Position.SalaryRange.MaxSalary
            })
            .Select(g => new SalaryRangeMetricDto
            {
                SalaryRangeId = g.Key.SalaryRangeId,
                RangeName = g.Key.Name,
                MinSalary = g.Key.MinSalary,
                MaxSalary = g.Key.MaxSalary,
                EmployeeCount = g.Count()
            })
            .OrderBy(s => s.MinSalary)
            .ToListAsync(ct);

        var genderTask = _context.Employees
            .AsNoTracking()
            .GroupBy(e => e.Gender)
            .Select(g => new { g.Key, Count = g.Count() })
            .ToListAsync(ct);

        var recentTask = _context.Employees
            .AsNoTracking()
            .OrderByDescending(e => e.CreatedAt)
            .Take(5)
            .Select(e => new RecentEmployeeDto
            {
                Id = e.Id,
                FullName = $"{e.FirstName} {e.LastName}",
                PositionTitle = e.Position.PositionTitle,
                DepartmentName = e.Department.Name,
                CreatedAt = e.CreatedAt ?? DateTime.MinValue
            })
            .ToListAsync(ct);

        await Task.WhenAll(
            totalEmployeesTask, totalDepartmentsTask, totalPositionsTask,
            totalSalaryRangesTask, newHiresTask, averageSalaryTask,
            deptTask, positionTask, salaryTask, genderTask, recentTask);

        var genderCounts = await genderTask;
        var dto = new DashboardMetricsDto
        {
            TotalEmployees = totalEmployeesTask.Result,
            TotalDepartments = totalDepartmentsTask.Result,
            TotalPositions = totalPositionsTask.Result,
            TotalSalaryRanges = totalSalaryRangesTask.Result,
            NewHiresThisMonth = newHiresTask.Result,
            AverageSalary = averageSalaryTask.Result ?? 0,
            EmployeesByDepartment = deptTask.Result,
            EmployeesByPosition = positionTask.Result,
            EmployeesBySalaryRange = salaryTask.Result,
            GenderDistribution = new GenderMetricDto
            {
                Male = genderCounts.FirstOrDefault(x => x.Key == Gender.Male)?.Count ?? 0,
                Female = genderCounts.FirstOrDefault(x => x.Key == Gender.Female)?.Count ?? 0
            },
            RecentEmployees = recentTask.Result
        };

        return dto;
    }
}
```

## Controller (WebApi)
**File:** `MyOnion/src/MyOnion.WebApi/Controllers/v1/DashboardController.cs`

```csharp
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/dashboard")]
[Authorize]
public sealed class DashboardController : BaseApiController
{
    [HttpGet("metrics")]
    [ProducesResponseType(typeof(Result<DashboardMetricsDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetMetrics()
        => Ok(await Mediator.Send(new GetDashboardMetricsQuery()));
}
```

## Caching Strategy (EasyCaching + Events)
- Cache the dashboard response for 5 minutes by key `Dashboard:Metrics`.
- Publish domain/application events on create/update/delete for Employees/Positions/Departments/SalaryRanges.
- Handle events in a dedicated cache invalidation handler to clear `Dashboard:Metrics` (and other related keys if needed).
- Use `EventDispatcher` to emit a `*ChangedEvent` after each successful write command; cache invalidation runs as the event handler.

## DI Registration
Add reader registration in `MyOnion.Infrastructure.Persistence` DI:
```
services.AddScoped<IDashboardMetricsReader, DashboardMetricsReader>();
```

## Testing Strategy
- Unit tests for the handler with a mocked reader.
- Integration test for `GET /api/v1/dashboard/metrics`.
- Verify caching behavior via `X-Cache-Status`.

## Implementation Steps
1. Add DTOs + query/handler in Application.
2. Add `DashboardMetricsReader` in Infrastructure.Persistence.
3. Register reader in DI.
4. Add `DashboardController`.
5. Add caching key + event-based invalidation handler.
6. Add tests and docs.
