using FluentAssertions;
using Moq;
using MyOnion.Application.Features.Positions.DTOs;
using MyOnion.Application.Features.Positions.Queries.GetPositions;
using MyOnion.Application.Interfaces;
using MyOnion.Application.Interfaces.Repositories;
using MyOnion.Application.Parameters;
using MyOnion.Domain.Entities;

namespace MyOnion.Application.Tests.Positions;

public class GetPositionsQueryHandlerTests
{
    private readonly Mock<IPositionRepositoryAsync> _repositoryMock = new();
    private readonly Mock<IModelHelper> _modelHelperMock = new();

    [Fact]
    public async Task Handle_ShouldReturnPagedResult_FromRepositoryShape()
    {
        // Arrange
        var query = new GetPositionsQuery
        {
            Fields = "Id,PositionTitle",
            PageNumber = 1,
            PageSize = 5
        };

        var shaped = new List<Entity>
        {
            new() { ["Id"] = Guid.NewGuid(), ["PositionTitle"] = "Developer" }
        };

        var records = new RecordsCount { RecordsFiltered = 1, RecordsTotal = 1 };

        _modelHelperMock.Setup(m => m.ValidateModelFields<PositionSummaryDto>(It.IsAny<string>()))
            .Returns("Id,PositionTitle");
        _modelHelperMock.Setup(m => m.GetModelFields<PositionSummaryDto>())
            .Returns("Id,PositionTitle");

        _repositoryMock.Setup(r => r.GetPositionReponseAsync(query))
            .ReturnsAsync((shaped, records));

        var handler = new GetAllPositionsQueryHandler(_repositoryMock.Object, _modelHelperMock.Object);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEquivalentTo(shaped);
        result.IsFailure.Should().BeFalse();
        _repositoryMock.Verify(r => r.GetPositionReponseAsync(query), Times.Once);
    }
}
