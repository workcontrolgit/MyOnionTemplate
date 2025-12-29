using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MyOnion.Application.Features.Departments.Queries.GetDepartments;
using MyOnion.Application.Helpers;
using MyOnion.Infrastructure.Persistence.Contexts;
using MyOnion.Infrastructure.Persistence.Repositories;
using MyOnion.Infrastructure.Shared.Services;
using MyOnion.Domain.Entities;

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
        _context.Departments.Add(new Department { Id = Guid.NewGuid(), Name = "Engineering" });
        await _context.SaveChangesAsync();

        var query = new GetDepartmentsQuery { Fields = "Id,Name", PageNumber = 1, PageSize = 5 };

        var (data, count) = await _repository.GetDepartmentResponseAsync(query);

        data.Should().HaveCount(1);
        data.First()["Name"].Should().Be("Engineering");
        count.RecordsFiltered.Should().Be(1);
        count.RecordsTotal.Should().Be(1);
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }
}
