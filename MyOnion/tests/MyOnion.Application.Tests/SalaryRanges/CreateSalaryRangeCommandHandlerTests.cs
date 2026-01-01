namespace MyOnion.Application.Tests.SalaryRanges;

public class CreateSalaryRangeCommandHandlerTests
{
    private readonly Mock<ISalaryRangeRepositoryAsync> _repositoryMock = new();
    private readonly Mock<IMapper> _mapperMock = new();

    [Fact]
    public async Task Handle_ShouldPersistSalaryRangeAndReturnId()
    {
        var command = new CreateSalaryRangeCommand
        {
            Name = "Level 1",
            MinSalary = 1000,
            MaxSalary = 2000
        };

        var salaryRange = new SalaryRange { Id = Guid.NewGuid(), Name = command.Name };

        _mapperMock.Setup(m => m.Map<SalaryRange>(command)).Returns(salaryRange);
        _repositoryMock.Setup(r => r.AddAsync(salaryRange)).ReturnsAsync(salaryRange);

        var handler = new CreateSalaryRangeCommand.CreateSalaryRangeCommandHandler(_repositoryMock.Object, _mapperMock.Object);

        var result = await handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(salaryRange.Id);
        _repositoryMock.Verify(r => r.AddAsync(salaryRange), Times.Once);
    }
}
