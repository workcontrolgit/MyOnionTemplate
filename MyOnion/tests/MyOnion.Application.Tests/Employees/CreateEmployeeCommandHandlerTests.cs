namespace MyOnion.Application.Tests.Employees;

public class CreateEmployeeCommandHandlerTests
{
    private readonly Mock<IEmployeeRepositoryAsync> _repositoryMock = new();
    private readonly Mock<IMapper> _mapperMock = new();

    [Fact]
    public async Task Handle_ShouldPersistEmployeeAndReturnId()
    {
        var command = new CreateEmployeeCommand
        {
            FirstName = "Jane",
            LastName = "Doe",
            Email = "jane@example.com",
            EmployeeNumber = "E-1",
            PositionId = Guid.NewGuid(),
            DepartmentId = Guid.NewGuid(),
            Birthday = DateTime.UtcNow.AddYears(-30)
        };

        var employee = new Employee { Id = Guid.NewGuid(), FirstName = command.FirstName };

        _mapperMock.Setup(m => m.Map<Employee>(command)).Returns(employee);
        _repositoryMock.Setup(r => r.AddAsync(employee)).ReturnsAsync(employee);

        var handler = new CreateEmployeeCommand.CreateEmployeeCommandHandler(_repositoryMock.Object, _mapperMock.Object);

        var result = await handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(employee.Id);
        _repositoryMock.Verify(r => r.AddAsync(employee), Times.Once);
    }
}
