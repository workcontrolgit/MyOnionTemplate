namespace MyOnion.Infrastructure.Tests.Repositories;

public class DepartmentRepositoryAsyncTests : IDisposable
{
    private readonly ApplicationDbContext _context;
    private readonly DepartmentRepositoryAsync _repository;

    public DepartmentRepositoryAsyncTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        var dateTime = new DateTimeService();
        var loggerFactory = LoggerFactory.Create(builder => { });
        _context = new ApplicationDbContext(options, dateTime, loggerFactory);
        _context.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.TrackAll;

        var dataShaper = new DataShapeHelper<Department>();
        _repository = new DepartmentRepositoryAsync(_context, dataShaper);
    }

    [Fact]
    public async Task GetDepartmentResponseAsync_ShouldReturnShapedData()
    {
        _context.Departments.Add(new Department { Id = Guid.NewGuid(), Name = new DepartmentName("Engineering") });
        await _context.SaveChangesAsync();

        var query = new GetDepartmentsQuery { Fields = "Id,Name", PageNumber = 1, PageSize = 5 };

        var (data, count) = await _repository.GetDepartmentResponseAsync(query);

        data.Should().HaveCount(1);
        data.First()["Name"].Should().Be("Engineering");
        count.RecordsFiltered.Should().Be(1);
        count.RecordsTotal.Should().Be(1);
    }

    [Fact]
    public async Task UpdateAsync_ShouldPersistChanges()
    {
        var department = new Department { Id = Guid.NewGuid(), Name = new DepartmentName("Ops") };
        _context.Departments.Add(department);
        await _context.SaveChangesAsync();

        department.Name = new DepartmentName("Operations");
        await _repository.UpdateAsync(department);

        var updated = await _context.Departments.FindAsync(department.Id);
        updated!.Name.Value.Should().Be("Operations");
    }

    [Fact]
    public async Task DeleteAsync_ShouldRemoveDepartment()
    {
        var department = new Department { Id = Guid.NewGuid(), Name = new DepartmentName("Temp") };
        _context.Departments.Add(department);
        await _context.SaveChangesAsync();

        await _repository.DeleteAsync(department);

        (await _context.Departments.FindAsync(department.Id)).Should().BeNull();
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }
}
