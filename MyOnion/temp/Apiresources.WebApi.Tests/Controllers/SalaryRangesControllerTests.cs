namespace $safeprojectname$.Controllers;

public class SalaryRangesControllerTests
{
    private readonly Mock<IMediator> _mediatorMock = new();
    private readonly SalaryRangesController _controller;

    public SalaryRangesControllerTests()
    {
        _controller = new SalaryRangesController();
        var services = new ServiceCollection();
        services.AddSingleton(_mediatorMock.Object);
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext
            {
                RequestServices = services.BuildServiceProvider()
            }
        };
    }

    [Fact]
    public async Task Get_ShouldReturnPagedSalaryRanges()
    {
        var query = new GetSalaryRangesQuery { PageNumber = 1, PageSize = 5 };
        var payload = PagedResult<IEnumerable<Entity>>.Success(new List<Entity> { new() }, 1, 5, new RecordsCount { RecordsFiltered = 1, RecordsTotal = 1 });
        _mediatorMock.Setup(m => m.Send(It.IsAny<GetSalaryRangesQuery>(), It.IsAny<CancellationToken>())).ReturnsAsync(payload);

        var result = await _controller.Get(query);

        var okResult = result as OkObjectResult;
        okResult.Should().NotBeNull();
        okResult!.Value.Should().Be(payload);
    }

    [Fact]
    public async Task Post_ShouldReturnCreatedResult()
    {
        var command = new CreateSalaryRangeCommand
        {
            Name = "Level 1",
            MinSalary = 1000,
            MaxSalary = 2000
        };
        var response = Result<Guid>.Success(Guid.NewGuid());
        _mediatorMock.Setup(m => m.Send(command, It.IsAny<CancellationToken>())).ReturnsAsync(response);

        var result = await _controller.Post(command);

        var created = result as CreatedAtActionResult;
        created.Should().NotBeNull();
        created!.Value.Should().Be(response);
        _mediatorMock.Verify(m => m.Send(command, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetById_ShouldReturnOk()
    {
        var id = Guid.NewGuid();
        var response = Result<SalaryRange>.Success(new SalaryRange { Id = id, Name = "Level 2" });
        _mediatorMock.Setup(m => m.Send(It.Is<GetSalaryRangeByIdQuery>(q => q.Id == id), It.IsAny<CancellationToken>())).ReturnsAsync(response);

        var result = await _controller.Get(id);

        var okResult = result as OkObjectResult;
        okResult.Should().NotBeNull();
        okResult!.Value.Should().Be(response);
    }

    [Fact]
    public async Task Put_ShouldReturnBadRequestWhenIdsMismatch()
    {
        var command = new UpdateSalaryRangeCommand { Id = Guid.NewGuid() };
        var result = await _controller.Put(Guid.NewGuid(), command);

        result.Should().BeOfType<BadRequestResult>();
    }
}

