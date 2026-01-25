namespace MyOnion.WebApi.Tests.Controllers;

public class DepartmentsControllerTests
{
    private readonly Mock<IMediator> _mediatorMock = new();
    private readonly DepartmentsController _controller;

    public DepartmentsControllerTests()
    {
        _controller = new DepartmentsController();
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
    public async Task Get_ShouldReturnOkResultWithPagedDepartments()
    {
        var query = new GetDepartmentsQuery { PageNumber = 1, PageSize = 10 };
        var payload = PagedResult<IEnumerable<Entity>>.Success(new List<Entity> { new() }, 1, 10, new RecordsCount { RecordsFiltered = 1, RecordsTotal = 1 });
        _mediatorMock.Setup(m => m.Send(It.IsAny<GetDepartmentsQuery>(), It.IsAny<CancellationToken>())).ReturnsAsync(payload);

        var result = await _controller.Get(query);

        var okResult = result as OkObjectResult;
        okResult.Should().NotBeNull();
        okResult!.Value.Should().Be(payload);
    }

    [Fact]
    public async Task Post_ShouldReturnCreatedResult()
    {
        var command = new CreateDepartmentCommand { Name = "Finance" };
        var response = Result<Guid>.Success(Guid.NewGuid());
        _mediatorMock.Setup(m => m.Send(command, It.IsAny<CancellationToken>())).ReturnsAsync(response);

        var result = await _controller.Post(command);

        var created = result as CreatedAtActionResult;
        created.Should().NotBeNull();
        created!.Value.Should().Be(response);
        _mediatorMock.Verify(m => m.Send(command, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetById_ShouldForwardQueryToMediator()
    {
        var id = Guid.NewGuid();
        var response = Result<Department>.Success(new Department { Id = id, Name = new DepartmentName("HR") });
        _mediatorMock.Setup(m => m.Send(It.Is<GetDepartmentByIdQuery>(q => q.Id == id), It.IsAny<CancellationToken>())).ReturnsAsync(response);

        var result = await _controller.Get(id);

        var okResult = result as OkObjectResult;
        okResult.Should().NotBeNull();
        okResult!.Value.Should().Be(response);
    }
}
