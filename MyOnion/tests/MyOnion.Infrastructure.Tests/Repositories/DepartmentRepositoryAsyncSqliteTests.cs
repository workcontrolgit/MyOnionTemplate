using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace MyOnion.Infrastructure.Tests.Repositories;

public class DepartmentRepositoryAsyncSqliteTests
{
    [Fact]
    public async Task GetDepartmentResponseAsync_ShouldFilterByNameValue()
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

        context.Departments.Add(new Department { Id = Guid.NewGuid(), Name = new DepartmentName("Engineering") });
        context.Departments.Add(new Department { Id = Guid.NewGuid(), Name = new DepartmentName("Operations") });
        await context.SaveChangesAsync();

        var repository = new DepartmentRepositoryAsync(context, new DataShapeHelper<Department>());
        var query = new GetDepartmentsQuery { Name = "Eng", Fields = "Id,Name", PageNumber = 1, PageSize = 10 };

        var (data, count) = await repository.GetDepartmentResponseAsync(query);

        data.Should().HaveCount(1);
        data.First()["Name"].Should().BeOfType<DepartmentName>().Which.Value.Should().Be("Engineering");
        count.RecordsFiltered.Should().Be(1);
        count.RecordsTotal.Should().Be(2);
    }
}
