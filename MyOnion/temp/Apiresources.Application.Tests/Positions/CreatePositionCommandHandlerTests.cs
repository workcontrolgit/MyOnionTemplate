namespace $ext_projectname$.Application.Tests.Positions;

public class CreatePositionCommandHandlerTests
{
    private readonly Mock<IPositionRepositoryAsync> _repositoryMock = new();
    private readonly Mock<IMapper> _mapperMock = new();

    [Fact]
    public async Task Handle_ShouldPersistPositionAndReturnSuccess()
    {
        // Arrange
        var command = new CreatePositionCommand
        {
            DepartmentId = Guid.NewGuid(),
            SalaryRangeId = Guid.NewGuid(),
            PositionNumber = "DEV-01",
            PositionTitle = "Developer",
            PositionDescription = "Writes awesome code"
        };

        var position = new Position
        {
            Id = Guid.NewGuid(),
            PositionNumber = command.PositionNumber,
            PositionTitle = command.PositionTitle
        };

        _mapperMock.Setup(m => m.Map<Position>(command)).Returns(position);
        _repositoryMock.Setup(r => r.AddAsync(position)).ReturnsAsync(position);

        var handler = new CreatePositionCommandHandler(_repositoryMock.Object, _mapperMock.Object);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(position.Id);
        _repositoryMock.Verify(r => r.AddAsync(position), Times.Once);
    }
}

