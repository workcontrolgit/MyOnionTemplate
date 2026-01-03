namespace $ext_projectname$.Application.Tests.Departments;

public class GetDepartmentsQueryHandlerTests
{
    private readonly Mock<IDepartmentRepositoryAsync> _repositoryMock = new();
    private readonly Mock<IModelHelper> _modelHelperMock = new();

    [Fact]
    public async Task Handle_ShouldReturnPagedDepartments()
    {
        var query = new GetDepartmentsQuery { Fields = "Id,Name", PageNumber = 1, PageSize = 10 };
        var shaped = new List<Entity> { new() { ["Id"] = Guid.NewGuid(), ["Name"] = "Engineering" } };
        var records = new RecordsCount { RecordsFiltered = 1, RecordsTotal = 1 };

        _modelHelperMock.Setup(m => m.ValidateModelFields<GetDepartmentsViewModel>(It.IsAny<string>())).Returns("Id,Name");
        _modelHelperMock.Setup(m => m.GetModelFields<GetDepartmentsViewModel>()).Returns("Id,Name");
        _repositoryMock.Setup(r => r.GetDepartmentResponseAsync(query)).ReturnsAsync((shaped, records));

        var handler = new GetAllDepartmentsQueryHandler(_repositoryMock.Object, _modelHelperMock.Object);

        var result = await handler.Handle(query, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEquivalentTo(shaped);
        _repositoryMock.Verify(r => r.GetDepartmentResponseAsync(query), Times.Once);
    }
}

