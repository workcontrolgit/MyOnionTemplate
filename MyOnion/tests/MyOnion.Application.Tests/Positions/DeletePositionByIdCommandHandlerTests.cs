using FluentAssertions;
using Moq;
using MyOnion.Application.Exceptions;
using MyOnion.Application.Features.Positions.Commands.DeletePositionById;
using MyOnion.Application.Interfaces.Repositories;
using MyOnion.Domain.Entities;

namespace MyOnion.Application.Tests.Positions;

public class DeletePositionByIdCommandHandlerTests
{
    private readonly Mock<IPositionRepositoryAsync> _repositoryMock = new();

    [Fact]
    public async Task Handle_ShouldDeletePosition()
    {
        var command = new DeletePositionByIdCommand { Id = Guid.NewGuid() };
        var entity = new Position { Id = command.Id };
        _repositoryMock.Setup(r => r.GetByIdAsync(command.Id)).ReturnsAsync(entity);

        var handler = new DeletePositionByIdCommand.DeletePositionByIdCommandHandler(_repositoryMock.Object);
        var result = await handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        _repositoryMock.Verify(r => r.DeleteAsync(entity), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldThrowWhenMissing()
    {
        var command = new DeletePositionByIdCommand { Id = Guid.NewGuid() };
        _repositoryMock.Setup(r => r.GetByIdAsync(command.Id)).ReturnsAsync((Position)null!);

        var handler = new DeletePositionByIdCommand.DeletePositionByIdCommandHandler(_repositoryMock.Object);

        await FluentActions.Awaiting(() => handler.Handle(command, CancellationToken.None))
            .Should().ThrowAsync<ApiException>()
            .WithMessage("Position Not Found.");
    }
}
