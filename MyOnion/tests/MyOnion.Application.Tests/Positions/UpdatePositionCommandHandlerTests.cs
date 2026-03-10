namespace MyOnion.Application.Tests.Positions;

public class UpdatePositionCommandHandlerTests
{
    private readonly Mock<IPositionRepositoryAsync> _repositoryMock = new();
    private readonly Mock<IEventDispatcher> _eventDispatcherMock = new();

    [Fact]
    public async Task Handle_ShouldUpdatePosition()
    {
        var departmentId = Guid.NewGuid();
        var salaryRangeId = Guid.NewGuid();
        var command = new UpdatePositionCommand
        {
            Id = Guid.NewGuid(),
            PositionTitle = "Updated",
            PositionNumber = "POS-002",
            PositionDescription = "Updated description",
            DepartmentId = departmentId,
            SalaryRangeId = salaryRangeId
        };

        var position = new Position
        {
            Id = command.Id,
            PositionTitle = new PositionTitle("Old"),
            PositionNumber = "POS-001",
            PositionDescription = "Old desc",
            DepartmentId = Guid.NewGuid(),
            SalaryRangeId = Guid.NewGuid()
        };
        _repositoryMock.Setup(r => r.GetByIdAsync(command.Id)).ReturnsAsync(position);

        var handler = new UpdatePositionCommand.UpdatePositionCommandHandler(
            _repositoryMock.Object,
            _eventDispatcherMock.Object);

        var result = await handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        position.PositionTitle.Value.Should().Be("Updated");
        position.PositionNumber.Should().Be("POS-002");
        position.PositionDescription.Should().Be("Updated description");
        position.DepartmentId.Should().Be(departmentId);
        position.SalaryRangeId.Should().Be(salaryRangeId);
        _repositoryMock.Verify(r => r.UpdateAsync(position), Times.Once);
        _eventDispatcherMock.Verify(s => s.PublishAsync(
            It.Is<PositionChangedEvent>(e => e.PositionId == position.Id),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldThrowWhenPositionMissing()
    {
        var command = new UpdatePositionCommand { Id = Guid.NewGuid() };
        _repositoryMock.Setup(r => r.GetByIdAsync(command.Id)).ReturnsAsync((Position)null!);
        var handler = new UpdatePositionCommand.UpdatePositionCommandHandler(
            _repositoryMock.Object,
            _eventDispatcherMock.Object);

        await FluentActions.Awaiting(() => handler.Handle(command, CancellationToken.None))
            .Should().ThrowAsync<ApiException>()
            .WithMessage("Position Not Found.");
    }
}
