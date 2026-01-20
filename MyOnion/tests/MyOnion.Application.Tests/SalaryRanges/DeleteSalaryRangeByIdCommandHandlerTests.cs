namespace MyOnion.Application.Tests.SalaryRanges;

public class DeleteSalaryRangeByIdCommandHandlerTests
{
    private readonly Mock<ISalaryRangeRepositoryAsync> _repositoryMock = new();
    private readonly Mock<IEventDispatcher> _eventDispatcherMock = new();

    [Fact]
    public async Task Handle_ShouldDeleteSalaryRange()
    {
        var command = new DeleteSalaryRangeByIdCommand { Id = Guid.NewGuid() };
        var entity = new SalaryRange { Id = command.Id };
        _repositoryMock.Setup(r => r.GetByIdAsync(command.Id)).ReturnsAsync(entity);

        var handler = new DeleteSalaryRangeByIdCommand.DeleteSalaryRangeByIdCommandHandler(
            _repositoryMock.Object,
            _eventDispatcherMock.Object);
        var result = await handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        _repositoryMock.Verify(r => r.DeleteAsync(entity), Times.Once);
        _eventDispatcherMock.Verify(s => s.PublishAsync(
            It.Is<SalaryRangeChangedEvent>(e => e.SalaryRangeId == entity.Id),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldThrowWhenNotFound()
    {
        var command = new DeleteSalaryRangeByIdCommand { Id = Guid.NewGuid() };
        _repositoryMock.Setup(r => r.GetByIdAsync(command.Id)).ReturnsAsync((SalaryRange)null!);

        var handler = new DeleteSalaryRangeByIdCommand.DeleteSalaryRangeByIdCommandHandler(
            _repositoryMock.Object,
            _eventDispatcherMock.Object);

        await FluentActions.Awaiting(() => handler.Handle(command, CancellationToken.None))
            .Should().ThrowAsync<ApiException>()
            .WithMessage("SalaryRange Not Found.");
    }
}
