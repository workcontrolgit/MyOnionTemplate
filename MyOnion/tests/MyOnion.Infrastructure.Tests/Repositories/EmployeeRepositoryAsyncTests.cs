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
    public async Task UpdateAsync_ShouldPersistEmployeeChanges()
    {
        var position = new Position
        {
            Id = Guid.NewGuid(),
            PositionTitle = "Support",
            PositionNumber = "SUP-1",
            PositionDescription = "Supports",
            DepartmentId = Guid.NewGuid(),
            SalaryRangeId = Guid.NewGuid()
        };
        _context.Positions.Add(position);
        var employee = new Employee
        {
            Id = Guid.NewGuid(),
            FirstName = "Alan",
            LastName = "Turing",
            Email = "alan@example.com",
            EmployeeNumber = "E-3",
            PositionId = position.Id,
            Salary = 3000,
            Birthday = DateTime.UtcNow.AddYears(-40)
        };
        _context.Employees.Add(employee);
        await _context.SaveChangesAsync();

        employee.FirstName = "Albert";
        await _repository.UpdateAsync(employee);

        var updated = await _context.Employees.FindAsync(employee.Id);
        updated!.FirstName.Should().Be("Albert");
    }

    [Fact]
    public async Task DeleteAsync_ShouldRemoveEmployee()
    {
        var position = new Position
        {
            Id = Guid.NewGuid(),
            PositionTitle = "Support",
            PositionNumber = "SUP-2",
            PositionDescription = "Supports",
            DepartmentId = Guid.NewGuid(),
            SalaryRangeId = Guid.NewGuid()
        };
        _context.Positions.Add(position);
        var employee = new Employee
        {
            Id = Guid.NewGuid(),
            FirstName = "Beth",
            LastName = "Smith",
            Email = "beth@example.com",
            EmployeeNumber = "E-4",
            PositionId = position.Id,
            Salary = 3200,
            Birthday = DateTime.UtcNow.AddYears(-35)
        };
        _context.Employees.Add(employee);
        await _context.SaveChangesAsync();

        await _repository.DeleteAsync(employee);

        (await _context.Employees.FindAsync(employee.Id)).Should().BeNull();
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }
}
