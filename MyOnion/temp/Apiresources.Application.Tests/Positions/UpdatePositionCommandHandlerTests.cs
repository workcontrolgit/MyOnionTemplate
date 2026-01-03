namespace $ext_projectname$.Application.Tests.Positions;

public class UpdatePositionCommandHandlerTests
{
    private readonly Mock<IPositionRepositoryAsync> _repositoryMock = new();

    [Fact]
    public async Task Handle_ShouldUpdatePosition()
    {
        var command = new UpdatePositionCommand
        {
            Id = Guid.NewGuid(),
            PositionTitle = "Updated",
            PositionDescription = "Updated description"
        };

        var position = new Position { Id = command.Id, PositionTitle = "Old", PositionDescription = "Old desc" };
        _repositoryMock.Setup(r => r.GetByIdAsync(command.Id)).ReturnsAsync(position);

        var handler = new UpdatePositionCommand.UpdatePositionCommandHandler(_repositoryMock.Object);

        var result = await handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        position.PositionTitle.Should().Be("Updated");
        _repositoryMock.Verify(r => r.UpdateAsync(position), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldThrowWhenPositionMissing()
    {
        var command = new UpdatePositionCommand { Id = Guid.NewGuid() };
        _repositoryMock.Setup(r => r.GetByIdAsync(command.Id)).ReturnsAsync((Position)null!);
        var handler = new UpdatePositionCommand.UpdatePositionCommandHandler(_repositoryMock.Object);

        await FluentActions.Awaiting(() => handler.Handle(command, CancellationToken.None))
            .Should().ThrowAsync<ApiException>()
            .WithMessage("Position Not Found.");
    }
}

