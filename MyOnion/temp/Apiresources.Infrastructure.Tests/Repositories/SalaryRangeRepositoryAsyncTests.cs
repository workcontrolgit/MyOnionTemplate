namespace $safeprojectname$.Repositories;

public class SalaryRangeRepositoryAsyncTests : IDisposable
{
    private readonly ApplicationDbContext _context;
    private readonly SalaryRangeRepositoryAsync _repository;

    public SalaryRangeRepositoryAsyncTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        var dateTime = new DateTimeService();
        var loggerFactory = LoggerFactory.Create(builder => { });
        _context = new ApplicationDbContext(options, dateTime, loggerFactory);
        _context.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.TrackAll;

        var dataShaper = new DataShapeHelper<SalaryRange>();
        _repository = new SalaryRangeRepositoryAsync(_context, dataShaper);
    }

    [Fact]
    public async Task GetSalaryRangeResponseAsync_ShouldReturnShapedData()
    {
        _context.SalaryRanges.Add(new SalaryRange { Id = Guid.NewGuid(), Name = "Level 1", MinSalary = 1000, MaxSalary = 2000 });
        await _context.SaveChangesAsync();

        var query = new GetSalaryRangesQuery { Fields = "Id,Name", PageNumber = 1, PageSize = 5 };

        var (data, count) = await _repository.GetSalaryRangeResponseAsync(query);

        data.Should().HaveCount(1);
        data.First()["Name"].Should().Be("Level 1");
        count.RecordsFiltered.Should().Be(1);
        count.RecordsTotal.Should().Be(1);
    }

    [Fact]
    public async Task UpdateAsync_ShouldPersistChanges()
    {
        var salaryRange = new SalaryRange { Id = Guid.NewGuid(), Name = "L1", MinSalary = 500, MaxSalary = 1000 };
        _context.SalaryRanges.Add(salaryRange);
        await _context.SaveChangesAsync();

        salaryRange.Name = "L1 Updated";
        await _repository.UpdateAsync(salaryRange);

        var updated = await _context.SalaryRanges.FindAsync(salaryRange.Id);
        updated!.Name.Should().Be("L1 Updated");
    }

    [Fact]
    public async Task DeleteAsync_ShouldRemoveSalaryRange()
    {
        var salaryRange = new SalaryRange { Id = Guid.NewGuid(), Name = "L2", MinSalary = 1500, MaxSalary = 2500 };
        _context.SalaryRanges.Add(salaryRange);
        await _context.SaveChangesAsync();

        await _repository.DeleteAsync(salaryRange);

        (await _context.SalaryRanges.FindAsync(salaryRange.Id)).Should().BeNull();
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }
}

