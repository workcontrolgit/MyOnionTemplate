using FluentAssertions;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using MyOnion.Application.Common.Results;
using MyOnion.Application.Features.Positions.Commands.CreatePosition;
using MyOnion.Application.Features.Positions.Queries.GetPositions;
using MyOnion.Application.Parameters;
using MyOnion.Domain.Entities;
using MyOnion.WebApi.Controllers.v1;

namespace MyOnion.WebApi.Tests.Controllers;

public class PositionsControllerTests
{
    private readonly Mock<IMediator> _mediatorMock = new();
    private readonly PositionsController _controller;

    public PositionsControllerTests()
    {
        _controller = new PositionsController();
        var services = new ServiceCollection();
        services.AddSingleton(_mediatorMock.Object);
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext
            {
                RequestServices = services.BuildServiceProvider()
            }
        };
    }

    [Fact]
    public async Task Get_ShouldReturnOkResultWithPagedData()
    {
        var query = new GetPositionsQuery { PageNumber = 1, PageSize = 10 };
        var payload = PagedResult<IEnumerable<Entity>>.Success(new List<Entity> { new() }, 1, 10, new RecordsCount { RecordsFiltered = 1, RecordsTotal = 1 });
        _mediatorMock.Setup(m => m.Send(It.IsAny<GetPositionsQuery>(), It.IsAny<CancellationToken>())).ReturnsAsync(payload);

        var result = await _controller.Get(query);

        var okResult = result as OkObjectResult;
        okResult.Should().NotBeNull();
        okResult!.Value.Should().Be(payload);
    }

    [Fact]
    public async Task Post_ShouldInvokeMediatorWithCommand()
    {
        var command = new CreatePositionCommand
        {
            PositionNumber = "PRD-001",
            PositionTitle = "Product Manager",
            PositionDescription = "Leads product",
            DepartmentId = Guid.NewGuid(),
            SalaryRangeId = Guid.NewGuid()
        };

        var response = Result<Guid>.Success(Guid.NewGuid());
        _mediatorMock.Setup(m => m.Send(command, It.IsAny<CancellationToken>())).ReturnsAsync(response);

        var result = await _controller.Post(command);

        var created = result as CreatedAtActionResult;
        created.Should().NotBeNull();
        created!.Value.Should().Be(response);
        _mediatorMock.Verify(m => m.Send(command, It.IsAny<CancellationToken>()), Times.Once);
    }
}
