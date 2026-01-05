namespace MyOnion.WebApi.Tests.Controllers;

public class EmployeesControllerTests
{
    private readonly Mock<IMediator> _mediatorMock = new();
    private readonly EmployeesController _controller;

    public EmployeesControllerTests()
    {
        _controller = new EmployeesController();
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
    public async Task Get_ShouldReturnPagedEmployees()
    {
        var query = new GetEmployeesQuery { PageNumber = 1, PageSize = 5 };
        var payload = PagedResult<IEnumerable<Entity>>.Success(new List<Entity> { new() }, 1, 5, new RecordsCount { RecordsFiltered = 1, RecordsTotal = 1 });
        _mediatorMock.Setup(m => m.Send(It.IsAny<GetEmployeesQuery>(), It.IsAny<CancellationToken>())).ReturnsAsync(payload);

        var result = await _controller.Get(query);

        var okResult = result as OkObjectResult;
        okResult.Should().NotBeNull();
        okResult!.Value.Should().Be(payload);
    }

    [Fact]
    public async Task Post_ShouldReturnCreatedResult()
    {
        var command = new CreateEmployeeCommand
        {
            FirstName = "Jane",
            LastName = "Doe",
            Email = "jane@example.com",
            EmployeeNumber = "E-100",
            PositionId = Guid.NewGuid(),
            Salary = 5000,
            Birthday = DateTime.UtcNow.AddYears(-30)
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
    public async Task Delete_ShouldInvokeMediator()
    {
        var id = Guid.NewGuid();
        var response = Result<Guid>.Success(id);
        _mediatorMock.Setup(m => m.Send(It.Is<DeleteEmployeeByIdCommand>(c => c.Id == id), It.IsAny<CancellationToken>())).ReturnsAsync(response);

        var result = await _controller.Delete(id);

        var okResult = result as OkObjectResult;
        okResult.Should().NotBeNull();
        okResult!.Value.Should().Be(response);
    }

    [Fact]
    public async Task Put_WithMismatchedIds_ShouldReturnBadRequest()
    {
        var command = new UpdateEmployeeCommand { Id = Guid.NewGuid() };
        var result = await _controller.Put(Guid.NewGuid(), command);

        result.Should().BeOfType<BadRequestResult>();
    }
}
