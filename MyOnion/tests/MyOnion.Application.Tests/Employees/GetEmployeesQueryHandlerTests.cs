using FluentAssertions;
using Moq;
using MyOnion.Application.Features.Employees.Queries.GetEmployees;
using MyOnion.Application.Interfaces;
using MyOnion.Application.Interfaces.Repositories;
using MyOnion.Application.Parameters;
using MyOnion.Domain.Entities;

namespace MyOnion.Application.Tests.Employees;

public class GetEmployeesQueryHandlerTests
{
    private readonly Mock<IEmployeeRepositoryAsync> _repositoryMock = new();
    private readonly Mock<IModelHelper> _modelHelperMock = new();

    [Fact]
    public async Task Handle_ShouldReturnPagedEmployees()
    {
        var query = new GetEmployeesQuery { Fields = "Id,FirstName", PageNumber = 1, PageSize = 10 };
        var shaped = new List<Entity> { new() { ["Id"] = Guid.NewGuid(), ["FirstName"] = "Jane" } };
        var records = new RecordsCount { RecordsFiltered = 1, RecordsTotal = 1 };

        _modelHelperMock.Setup(m => m.ValidateModelFields<GetEmployeesViewModel>(It.IsAny<string>())).Returns("Id,FirstName");
        _modelHelperMock.Setup(m => m.GetModelFields<GetEmployeesViewModel>()).Returns("Id,FirstName");
        _repositoryMock.Setup(r => r.GetEmployeeResponseAsync(query)).ReturnsAsync((shaped, records));

        var handler = new GetAllEmployeesQueryHandler(_repositoryMock.Object, _modelHelperMock.Object);

        var result = await handler.Handle(query, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEquivalentTo(shaped);
        _repositoryMock.Verify(r => r.GetEmployeeResponseAsync(query), Times.Once);
    }
}
