using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MyOnion.Application.Features.Employees.Queries.GetEmployees;
using MyOnion.Application.Helpers;
using MyOnion.Application.Parameters;
using MyOnion.Domain.Entities;
using MyOnion.Infrastructure.Persistence.Contexts;
using MyOnion.Infrastructure.Persistence.Repositories;
using MyOnion.Infrastructure.Shared.Services;

namespace MyOnion.Infrastructure.Tests.Repositories;

public class EmployeeRepositoryAsyncTests : IDisposable
{
    private readonly ApplicationDbContext _context;
    private readonly EmployeeRepositoryAsync _repository;

    public EmployeeRepositoryAsyncTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        var dateTime = new DateTimeService();
        var loggerFactory = LoggerFactory.Create(builder => { });
        _context = new ApplicationDbContext(options, dateTime, loggerFactory);
        _context.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.TrackAll;

        var dataShaper = new DataShapeHelper<Employee>();
        _repository = new EmployeeRepositoryAsync(_context, dataShaper);
    }

    [Fact]
    public async Task GetEmployeeResponseAsync_ShouldReturnShapedData()
    {
        var position = new Position
        {
            Id = Guid.NewGuid(),
            PositionTitle = "Developer",
            PositionNumber = "DEV-1",
            PositionDescription = "Writes code",
            DepartmentId = Guid.NewGuid(),
            SalaryRangeId = Guid.NewGuid()
        };
        _context.Positions.Add(position);
        _context.Employees.Add(new Employee
        {
            Id = Guid.NewGuid(),
            FirstName = "Jane",
            LastName = "Doe",
            Email = "jane@example.com",
            EmployeeNumber = "E-1",
            PositionId = position.Id,
            Salary = 5000,
            Birthday = DateTime.UtcNow.AddYears(-30)
        });
        await _context.SaveChangesAsync();

        var query = new GetEmployeesQuery { Fields = "Id,FirstName", PageNumber = 1, PageSize = 10 };

        var (data, count) = await _repository.GetEmployeeResponseAsync(query);

        data.Should().HaveCount(1);
        data.First()["FirstName"].Should().Be("Jane");
        count.Should().BeEquivalentTo(new RecordsCount { RecordsFiltered = 1, RecordsTotal = 1 });
    }

    [Fact]
    public async Task GetPagedEmployeeResponseAsync_ShouldReturnPagedDataTableShape()
    {
        var position = new Position
        {
            Id = Guid.NewGuid(),
            PositionTitle = "QA",
            PositionNumber = "QA-1",
            PositionDescription = "Tests code",
            DepartmentId = Guid.NewGuid(),
            SalaryRangeId = Guid.NewGuid()
        };
        _context.Positions.Add(position);
        _context.Employees.Add(new Employee
        {
            Id = Guid.NewGuid(),
            FirstName = "John",
            LastName = "Smith",
            Email = "john@example.com",
            EmployeeNumber = "E-2",
            PositionId = position.Id,
            Salary = 4000,
            Birthday = DateTime.UtcNow.AddYears(-25)
        });
        await _context.SaveChangesAsync();

        var query = new PagedEmployeesQuery
        {
            Fields = "Id,FirstName",
            Draw = 1,
            Start = 0,
            Length = 10,
            Order = new List<SortOrder> { new() { Column = 0, Dir = "asc" } },
            Columns = new List<Column>(),
            Search = new Search { Value = string.Empty, Regex = false }
        };

        var (data, count) = await _repository.GetPagedEmployeeResponseAsync(query);

        data.Should().HaveCount(1);
        data.First()["FirstName"].Should().Be("John");
        count.Should().BeEquivalentTo(new RecordsCount { RecordsFiltered = 1, RecordsTotal = 1 });
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }
}
