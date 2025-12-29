using FluentAssertions;
using Moq;
using MyOnion.Application.Exceptions;
using MyOnion.Application.Features.Departments.Commands.UpdateDepartment;
using MyOnion.Application.Interfaces.Repositories;
using MyOnion.Domain.Entities;

namespace MyOnion.Application.Tests.Departments;

public class UpdateDepartmentCommandHandlerTests
{
    private readonly Mock<IDepartmentRepositoryAsync> _repositoryMock = new();

    [Fact]
    public async Task Handle_ShouldUpdateDepartment()
    {
        var command = new UpdateDepartmentCommand { Id = Guid.NewGuid(), Name = "Finance" };
        var department = new Department { Id = command.Id, Name = "Old" };
        _repositoryMock.Setup(r => r.GetByIdAsync(command.Id)).ReturnsAsync(department);

        var handler = new UpdateDepartmentCommand.UpdateDepartmentCommandHandler(_repositoryMock.Object);
        var result = await handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        department.Name.Should().Be("Finance");
        _repositoryMock.Verify(r => r.UpdateAsync(department), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldThrowWhenDepartmentMissing()
    {
        var command = new UpdateDepartmentCommand { Id = Guid.NewGuid(), Name = "Finance" };
        _repositoryMock.Setup(r => r.GetByIdAsync(command.Id)).ReturnsAsync((Department)null!);

        var handler = new UpdateDepartmentCommand.UpdateDepartmentCommandHandler(_repositoryMock.Object);

        await FluentActions.Awaiting(() => handler.Handle(command, CancellationToken.None))
            .Should().ThrowAsync<ApiException>()
            .WithMessage("Department Not Found.");
    }
}
