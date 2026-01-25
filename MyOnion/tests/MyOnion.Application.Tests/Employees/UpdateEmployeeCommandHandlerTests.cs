namespace MyOnion.Application.Tests.Employees;

public class UpdateEmployeeCommandHandlerTests
{
    private readonly Mock<IEmployeeRepositoryAsync> _repositoryMock = new();
    private readonly Mock<IEventDispatcher> _eventDispatcherMock = new();

    [Fact]
    public async Task Handle_ShouldUpdateEmployee()
    {
        var command = new UpdateEmployeeCommand
        {
            Id = Guid.NewGuid(),
            FirstName = "Jane",
            LastName = "Doe",
            Email = "jane@example.com",
            EmployeeNumber = "E-1",
            PositionId = Guid.NewGuid(),
            DepartmentId = Guid.NewGuid(),
            Salary = 5000,
            Birthday = DateTime.UtcNow.AddYears(-30)
        };

        var employee = new Employee { Id = command.Id, Name = new PersonName("Old", null, "Name") };
        _repositoryMock.Setup(r => r.GetByIdAsync(command.Id)).ReturnsAsync(employee);

        var handler = new UpdateEmployeeCommand.UpdateEmployeeCommandHandler(
            _repositoryMock.Object,
            _eventDispatcherMock.Object);
        var result = await handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        employee.Name.FirstName.Should().Be("Jane");
        employee.DepartmentId.Should().Be(command.DepartmentId);
        _repositoryMock.Verify(r => r.UpdateAsync(employee), Times.Once);
        _eventDispatcherMock.Verify(s => s.PublishAsync(
            It.Is<EmployeeChangedEvent>(e => e.EmployeeId == employee.Id),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldThrowWhenMissing()
    {
        var command = new UpdateEmployeeCommand { Id = Guid.NewGuid() };
        _repositoryMock.Setup(r => r.GetByIdAsync(command.Id)).ReturnsAsync((Employee)null!);

        var handler = new UpdateEmployeeCommand.UpdateEmployeeCommandHandler(
            _repositoryMock.Object,
            _eventDispatcherMock.Object);

        await FluentActions.Awaiting(() => handler.Handle(command, CancellationToken.None))
            .Should().ThrowAsync<ApiException>()
            .WithMessage("Employee Not Found.");
    }
}
