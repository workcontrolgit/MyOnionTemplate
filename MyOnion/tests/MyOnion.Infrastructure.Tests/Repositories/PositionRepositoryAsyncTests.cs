using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using MyOnion.Application.Features.Positions.Queries.GetPositions;
using MyOnion.Application.Helpers;
using MyOnion.Application.Interfaces;
using MyOnion.Application.Interfaces.Repositories;
using MyOnion.Application.Parameters;
using MyOnion.Domain.Entities;
using MyOnion.Infrastructure.Persistence.Contexts;
using MyOnion.Infrastructure.Persistence.Repositories;
using MyOnion.Infrastructure.Shared.Services;

namespace MyOnion.Infrastructure.Tests.Repositories;

public class PositionRepositoryAsyncTests : IDisposable
{
    private readonly ApplicationDbContext _context;
    private readonly PositionRepositoryAsync _repository;

    public PositionRepositoryAsyncTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        var dateTime = new DateTimeService();
        var loggerFactory = LoggerFactory.Create(builder => { });
        _context = new ApplicationDbContext(options, dateTime, loggerFactory);
        _context.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.TrackAll;

        var dataShaper = new DataShapeHelper<Position>();
        var mockService = new Mock<IMockService>();

        _repository = new PositionRepositoryAsync(_context, dataShaper, mockService.Object);
    }

    [Fact]
    public async Task IsUniquePositionNumberAsync_ShouldDetectExistingNumber()
    {
        var position = new Position
        {
            Id = Guid.NewGuid(),
            PositionNumber = "ENG-01",
            PositionTitle = "Engineer",
            PositionDescription = "Builds things",
            DepartmentId = Guid.NewGuid(),
            SalaryRangeId = Guid.NewGuid()
        };

        _context.Positions.Add(position);
        await _context.SaveChangesAsync();

        (await _repository.IsUniquePositionNumberAsync("ENG-99")).Should().BeTrue();
        (await _repository.IsUniquePositionNumberAsync("ENG-01")).Should().BeFalse();
    }

    [Fact]
    public async Task GetPositionReponseAsync_ShouldShapeDataAndReturnRecordCounts()
    {
        _context.Positions.Add(new Position
        {
            Id = Guid.NewGuid(),
            PositionNumber = "QA-01",
            PositionTitle = "QA Engineer",
            PositionDescription = "Tests things",
            DepartmentId = Guid.NewGuid(),
            SalaryRangeId = Guid.NewGuid(),
            Department = new Department { Id = Guid.NewGuid(), Name = "Engineering" },
            SalaryRange = new SalaryRange { Id = Guid.NewGuid(), MinSalary = 1, MaxSalary = 2 }
        });
        await _context.SaveChangesAsync();

        var query = new GetPositionsQuery
        {
            Fields = "Id,PositionTitle",
            PageNumber = 1,
            PageSize = 10
        };

        var (data, count) = await _repository.GetPositionReponseAsync(query);

        data.Should().HaveCount(1);
        data.First()["PositionTitle"].Should().Be("QA Engineer");
        count.Should().BeEquivalentTo(new RecordsCount { RecordsFiltered = 1, RecordsTotal = 1 });
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }
}
