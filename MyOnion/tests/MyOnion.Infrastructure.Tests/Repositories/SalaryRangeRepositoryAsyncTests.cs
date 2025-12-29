using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MyOnion.Application.Features.SalaryRanges.Queries.GetSalaryRanges;
using MyOnion.Application.Helpers;
using MyOnion.Domain.Entities;
using MyOnion.Infrastructure.Persistence.Contexts;
using MyOnion.Infrastructure.Persistence.Repositories;
using MyOnion.Infrastructure.Shared.Services;

namespace MyOnion.Infrastructure.Tests.Repositories;

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

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }
}
