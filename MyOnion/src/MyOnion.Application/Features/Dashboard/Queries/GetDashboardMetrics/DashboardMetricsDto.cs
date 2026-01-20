#nullable enable
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
