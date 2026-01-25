using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace MyOnion.Infrastructure.Tests.Repositories;

public class PositionRepositoryAsyncSqliteTests
{
    [Fact]
    public async Task GetPositionResponseAsync_ShouldFilterByTitleAndDepartment()
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
        var salaryRange = new SalaryRange { Id = Guid.NewGuid(), MinSalary = 1, MaxSalary = 2 };
        context.Departments.Add(department);
        context.SalaryRanges.Add(salaryRange);
        context.Positions.Add(new Position
        {
            Id = Guid.NewGuid(),
            PositionNumber = "ENG-01",
            PositionTitle = new PositionTitle("QA Engineer"),
            PositionDescription = "Tests",
            DepartmentId = department.Id,
            SalaryRangeId = salaryRange.Id
        });
        await context.SaveChangesAsync();

        var repository = new PositionRepositoryAsync(context, new DataShapeHelper<Position>(), new Mock<IMockService>().Object);
        var query = new GetPositionsQuery
        {
            PositionTitle = "QA",
            Department = "Engineering",
            Fields = "Id,PositionTitle",
            PageNumber = 1,
            PageSize = 10
        };

        var (data, count) = await repository.GetPositionReponseAsync(query);

        data.Should().HaveCount(1);
        data.First()["PositionTitle"].Should().BeOfType<PositionTitle>().Which.Value.Should().Be("QA Engineer");
        count.RecordsFiltered.Should().Be(1);
        count.RecordsTotal.Should().Be(1);
    }
}
