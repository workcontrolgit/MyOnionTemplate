using MyOnion.Application.Features.Dashboard.Queries.GetDashboardMetrics;
using MyOnion.Application.Interfaces;
using MyOnion.Domain.Enums;

namespace MyOnion.Infrastructure.Persistence.Readers;

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

        var totalEmployees = await _context.Employees.CountAsync(ct);
        var totalDepartments = await _context.Departments.CountAsync(ct);
        var totalPositions = await _context.Positions.CountAsync(ct);
        var totalSalaryRanges = await _context.SalaryRanges.CountAsync(ct);
        var newHires = await _context.Employees.Where(e => e.Created >= startOfMonth).CountAsync(ct);
        var averageSalary = await _context.Employees.AverageAsync(e => (decimal?)e.Salary, ct);

        var deptTask = await _context.Employees
            .AsNoTracking()
            .GroupBy(e => new { e.DepartmentId, DepartmentName = e.Department.Name.Value })
            .Select(g => new DepartmentMetricDto
            {
                DepartmentId = g.Key.DepartmentId,
                DepartmentName = g.Key.DepartmentName,
                EmployeeCount = g.Count()
            })
            .OrderByDescending(d => d.EmployeeCount)
            .ToListAsync(ct);

        var positionTask = await _context.Employees
            .AsNoTracking()
            .GroupBy(e => new { e.PositionId, PositionTitle = e.Position.PositionTitle.Value })
            .Select(g => new PositionMetricDto
            {
                PositionId = g.Key.PositionId,
                PositionTitle = g.Key.PositionTitle,
                EmployeeCount = g.Count()
            })
            .OrderByDescending(p => p.EmployeeCount)
            .Take(10)
            .ToListAsync(ct);

        var salaryTask = await _context.Employees
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

        var genderTask = await _context.Employees
            .AsNoTracking()
            .GroupBy(e => e.Gender)
            .Select(g => new { Gender = g.Key, Count = g.Count() })
            .ToListAsync(ct);

        var recentTask = await _context.Employees
            .AsNoTracking()
            .OrderByDescending(e => e.Created)
            .Take(5)
            .Select(e => new RecentEmployeeDto
            {
                Id = e.Id,
                FullName = $"{e.Name.FirstName} {e.Name.LastName}",
                PositionTitle = e.Position.PositionTitle.Value,
                DepartmentName = e.Department.Name.Value,
                CreatedAt = e.Created
            })
            .ToListAsync(ct);

        var genderCounts = genderTask;
        return new DashboardMetricsDto
        {
            TotalEmployees = totalEmployees,
            TotalDepartments = totalDepartments,
            TotalPositions = totalPositions,
            TotalSalaryRanges = totalSalaryRanges,
            NewHiresThisMonth = newHires,
            AverageSalary = averageSalary ?? 0,
            EmployeesByDepartment = deptTask,
            EmployeesByPosition = positionTask,
            EmployeesBySalaryRange = salaryTask,
            GenderDistribution = new GenderMetricDto
            {
                Male = genderCounts.FirstOrDefault(x => x.Gender == Gender.Male)?.Count ?? 0,
                Female = genderCounts.FirstOrDefault(x => x.Gender == Gender.Female)?.Count ?? 0
            },
            RecentEmployees = recentTask
        };
    }
}
