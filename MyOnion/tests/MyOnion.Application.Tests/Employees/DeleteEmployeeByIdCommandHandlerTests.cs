namespace MyOnion.Application.Tests.Employees;

public class DeleteEmployeeByIdCommandHandlerTests
{
    private readonly Mock<IEmployeeRepositoryAsync> _repositoryMock = new();
    private readonly Mock<IEventDispatcher> _eventDispatcherMock = new();

    [Fact]
    public async Task Handle_ShouldDeleteEmployee()
    {
        var command = new DeleteEmployeeByIdCommand { Id = Guid.NewGuid() };
        var entity = new Employee { Id = command.Id };
        _repositoryMock.Setup(r => r.GetByIdAsync(command.Id)).ReturnsAsync(entity);

        var handler = new DeleteEmployeeByIdCommand.DeleteEmployeeByIdCommandHandler(
            _repositoryMock.Object,
            _eventDispatcherMock.Object);
        var result = await handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        _repositoryMock.Verify(r => r.DeleteAsync(entity), Times.Once);
        _eventDispatcherMock.Verify(s => s.PublishAsync(
            It.Is<EmployeeChangedEvent>(e => e.EmployeeId == entity.Id),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldThrowWhenNotFound()
    {
        var command = new DeleteEmployeeByIdCommand { Id = Guid.NewGuid() };
        _repositoryMock.Setup(r => r.GetByIdAsync(command.Id)).ReturnsAsync((Employee)null!);

        var handler = new DeleteEmployeeByIdCommand.DeleteEmployeeByIdCommandHandler(
            _repositoryMock.Object,
            _eventDispatcherMock.Object);

        await FluentActions.Awaiting(() => handler.Handle(command, CancellationToken.None))
            .Should().ThrowAsync<ApiException>()
            .WithMessage("Employee Not Found.");
    }
}
