using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using MyOnion.Infrastructure.Persistence.Readers;

namespace MyOnion.Infrastructure.Tests.Readers;

public class DashboardMetricsReaderSqliteTests
{
    [Fact]
    public async Task GetDashboardMetricsAsync_ReturnsCountsAndBreakdowns()
    {
        await using var connection = new SqliteConnection("DataSource=:memory:");
        await connection.OpenAsync();

        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseSqlite(connection)
            .Options;

        await using var context = new ApplicationDbContext(
            options,
            new DateTimeService(),
            LoggerFactory.Create(_ => { }));
        await context.Database.EnsureCreatedAsync();

        var department = new Department { Id = Guid.NewGuid(), Name = new DepartmentName("Engineering") };
        var salaryRange = new SalaryRange { Id = Guid.NewGuid(), Name = "L1", MinSalary = 1, MaxSalary = 2 };
        var position = new Position
        {
            Id = Guid.NewGuid(),
            PositionNumber = "ENG-01",
            PositionTitle = new PositionTitle("QA Engineer"),
            PositionDescription = "Tests",
            DepartmentId = department.Id,
            SalaryRangeId = salaryRange.Id
        };
        var employee = new Employee
        {
            Id = Guid.NewGuid(),
            Name = new PersonName("Ada", null, "Lovelace"),
            DepartmentId = department.Id,
            PositionId = position.Id,
            Salary = 100,
            Created = DateTime.UtcNow,
            EmployeeNumber = "E-1",
            Email = "ada@example.com",
            Birthday = DateTime.UtcNow.AddYears(-30)
        };

        context.Departments.Add(department);
        context.SalaryRanges.Add(salaryRange);
        context.Positions.Add(position);
        context.Employees.Add(employee);
        await context.SaveChangesAsync();

        var reader = new DashboardMetricsReader(context);

        var metrics = await reader.GetDashboardMetricsAsync(CancellationToken.None);

        metrics.TotalEmployees.Should().Be(1);
        metrics.TotalDepartments.Should().Be(1);
        metrics.TotalPositions.Should().Be(1);
        metrics.TotalSalaryRanges.Should().Be(1);
        metrics.EmployeesByDepartment.Should().ContainSingle(d => d.DepartmentName == "Engineering" && d.EmployeeCount == 1);
        metrics.EmployeesByPosition.Should().ContainSingle(p => p.PositionTitle == "QA Engineer" && p.EmployeeCount == 1);
    }
}
