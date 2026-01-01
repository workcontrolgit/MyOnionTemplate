namespace MyOnion.Application.Tests.SalaryRanges;

public class GetSalaryRangesQueryHandlerTests
{
    private readonly Mock<ISalaryRangeRepositoryAsync> _repositoryMock = new();
    private readonly Mock<IModelHelper> _modelHelperMock = new();

    [Fact]
    public async Task Handle_ShouldReturnPagedSalaryRanges()
    {
        var query = new GetSalaryRangesQuery { Fields = "Id,Name", PageNumber = 1, PageSize = 5 };
        var shaped = new List<Entity> { new() { ["Id"] = Guid.NewGuid(), ["Name"] = "Level 1" } };
        var records = new RecordsCount { RecordsFiltered = 1, RecordsTotal = 1 };

        _modelHelperMock.Setup(m => m.ValidateModelFields<GetSalaryRangesViewModel>(It.IsAny<string>())).Returns("Id,Name");
        _modelHelperMock.Setup(m => m.GetModelFields<GetSalaryRangesViewModel>()).Returns("Id,Name");
        _repositoryMock.Setup(r => r.GetSalaryRangeResponseAsync(query)).ReturnsAsync((shaped, records));

        var handler = new GetAllSalaryRangesQueryHandler(_repositoryMock.Object, _modelHelperMock.Object);

        var result = await handler.Handle(query, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEquivalentTo(shaped);
        _repositoryMock.Verify(r => r.GetSalaryRangeResponseAsync(query), Times.Once);
    }
}
