namespace MyOnion.Application.Tests.Departments;

public class CreateDepartmentCommandHandlerTests
{
    private readonly Mock<IDepartmentRepositoryAsync> _repositoryMock = new();
    private readonly Mock<IMapper> _mapperMock = new();
    private readonly Mock<IEventDispatcher> _eventDispatcherMock = new();

    [Fact]
    public async Task Handle_ShouldPersistDepartmentAndReturnId()
    {
        var command = new CreateDepartmentCommand { Name = "Engineering" };
        var department = new Department { Id = Guid.NewGuid(), Name = new DepartmentName(command.Name) };

        _mapperMock.Setup(m => m.Map<Department>(command)).Returns(department);
        _repositoryMock.Setup(r => r.AddAsync(department)).ReturnsAsync(department);

        var handler = new CreateDepartmentCommand.CreateDepartmentCommandHandler(
            _repositoryMock.Object,
            _mapperMock.Object,
            _eventDispatcherMock.Object);

        var result = await handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(department.Id);
        _repositoryMock.Verify(r => r.AddAsync(department), Times.Once);
        _eventDispatcherMock.Verify(s => s.PublishAsync(
            It.Is<DepartmentChangedEvent>(e => e.DepartmentId == department.Id),
            It.IsAny<CancellationToken>()), Times.Once);
    }
}
