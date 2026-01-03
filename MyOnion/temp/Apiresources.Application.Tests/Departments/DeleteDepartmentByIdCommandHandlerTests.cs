namespace $ext_projectname$.Application.Tests.Departments;

public class DeleteDepartmentByIdCommandHandlerTests
{
    private readonly Mock<IDepartmentRepositoryAsync> _repositoryMock = new();

    [Fact]
    public async Task Handle_ShouldDeleteDepartment()
    {
        var command = new DeleteDepartmentByIdCommand { Id = Guid.NewGuid() };
        var entity = new Department { Id = command.Id };
        _repositoryMock.Setup(r => r.GetByIdAsync(command.Id)).ReturnsAsync(entity);

        var handler = new DeleteDepartmentByIdCommand.DeleteDepartmentByIdCommandHandler(_repositoryMock.Object);
        var result = await handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        _repositoryMock.Verify(r => r.DeleteAsync(entity), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldThrowWhenNotFound()
    {
        var command = new DeleteDepartmentByIdCommand { Id = Guid.NewGuid() };
        _repositoryMock.Setup(r => r.GetByIdAsync(command.Id)).ReturnsAsync((Department)null!);

        var handler = new DeleteDepartmentByIdCommand.DeleteDepartmentByIdCommandHandler(_repositoryMock.Object);

        await FluentActions.Awaiting(() => handler.Handle(command, CancellationToken.None))
            .Should().ThrowAsync<ApiException>()
            .WithMessage("Department Not Found.");
    }
}

