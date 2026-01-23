namespace MyOnion.Application.Tests.SalaryRanges;

public class UpdateSalaryRangeCommandHandlerTests
{
    private readonly Mock<ISalaryRangeRepositoryAsync> _repositoryMock = new();
    private readonly Mock<IEventDispatcher> _eventDispatcherMock = new();

    [Fact]
    public async Task Handle_ShouldUpdateSalaryRange()
    {
        var command = new UpdateSalaryRangeCommand
        {
            Id = Guid.NewGuid(),
            Name = "Level 2",
            MinSalary = 2000,
            MaxSalary = 3000
        };

        var salaryRange = new SalaryRange { Id = command.Id };
        _repositoryMock.Setup(r => r.GetByIdAsync(command.Id)).ReturnsAsync(salaryRange);

        var handler = new UpdateSalaryRangeCommand.UpdateSalaryRangeCommandHandler(
            _repositoryMock.Object,
            _eventDispatcherMock.Object);
        var result = await handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        salaryRange.Name.Should().Be("Level 2");
        _repositoryMock.Verify(r => r.UpdateAsync(salaryRange), Times.Once);
        _eventDispatcherMock.Verify(s => s.PublishAsync(
            It.Is<SalaryRangeChangedEvent>(e => e.SalaryRangeId == salaryRange.Id),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldThrowWhenMissing()
    {
        var command = new UpdateSalaryRangeCommand { Id = Guid.NewGuid() };
        _repositoryMock.Setup(r => r.GetByIdAsync(command.Id)).ReturnsAsync((SalaryRange)null!);

        var handler = new UpdateSalaryRangeCommand.UpdateSalaryRangeCommandHandler(
            _repositoryMock.Object,
            _eventDispatcherMock.Object);

        await FluentActions.Awaiting(() => handler.Handle(command, CancellationToken.None))
            .Should().ThrowAsync<ApiException>()
            .WithMessage("SalaryRange Not Found.");
    }
}
