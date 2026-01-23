namespace MyOnion.Application.Tests.Positions;

public class DeletePositionByIdCommandHandlerTests
{
    private readonly Mock<IPositionRepositoryAsync> _repositoryMock = new();
    private readonly Mock<IEventDispatcher> _eventDispatcherMock = new();

    [Fact]
    public async Task Handle_ShouldDeletePosition()
    {
        var command = new DeletePositionByIdCommand { Id = Guid.NewGuid() };
        var entity = new Position { Id = command.Id };
        _repositoryMock.Setup(r => r.GetByIdAsync(command.Id)).ReturnsAsync(entity);

        var handler = new DeletePositionByIdCommand.DeletePositionByIdCommandHandler(
            _repositoryMock.Object,
            _eventDispatcherMock.Object);
        var result = await handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        _repositoryMock.Verify(r => r.DeleteAsync(entity), Times.Once);
        _eventDispatcherMock.Verify(s => s.PublishAsync(
            It.Is<PositionChangedEvent>(e => e.PositionId == entity.Id),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldThrowWhenMissing()
    {
        var command = new DeletePositionByIdCommand { Id = Guid.NewGuid() };
        _repositoryMock.Setup(r => r.GetByIdAsync(command.Id)).ReturnsAsync((Position)null!);

        var handler = new DeletePositionByIdCommand.DeletePositionByIdCommandHandler(
            _repositoryMock.Object,
            _eventDispatcherMock.Object);

        await FluentActions.Awaiting(() => handler.Handle(command, CancellationToken.None))
            .Should().ThrowAsync<ApiException>()
            .WithMessage("Position Not Found.");
    }
}
