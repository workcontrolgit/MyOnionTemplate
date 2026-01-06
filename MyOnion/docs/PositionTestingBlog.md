# Testing the Positions Feature in Template OnionAPI

Template OnionAPI’s Positions feature spans handlers, repositories, and controllers, so the testing story has to cover each layer. This blog walks through what the testing plan includes, why it matters, and example code pulled from the real test projects so you can mirror the approach in your own .NET solutions.

## What Gets Tested

The testing plan outlined in Positions-Unit-Testing.md focuses on three slices:

1. **Application layer** – MediatR handlers for commands (create/update/delete) and queries (get-by-id/list). Tests assert success responses, validation errors, and paging behavior.
2. **Infrastructure layer** – Repository operations (PositionRepositoryAsync), specification filters, and data shaping with an EF Core InMemory context.
3. **Web API layer** – Controller endpoints through WebApplicationFactory or mocked mediators to verify payloads, status codes, and middleware artifacts like timing headers.

## Why This Matters

- **Confidence across layers** – Catch regressions in the handler, repository, or controller before they compound into production issues.
- **Specification coverage** – The plan explicitly calls out spec filters and data shaping helpers, ensuring dynamic field selection stays correct when filtering is refactored.
- **Middleware verification** – Even infrastructure-level timing headers (from the Endpoint Timing plan) get asserted in API tests, so observability remains intact.
- **CI readiness** – All three test projects (Application.Tests, Infrastructure.Tests, WebApi.Tests) are part of the solution, making dotnet test a single command in CI.

## Example: Application Handler Test

`csharp
public class GetPositionsQueryHandlerTests
{
    [Fact]
    public async Task Handle_ReturnsPagedResultWithData()
    {
        var repo = new Mock<IPositionRepositoryAsync>();
        var modelHelper = new Mock<IModelHelper>();
        var recordCount = new RecordsCount { RecordsFiltered = 2, RecordsTotal = 2 };

        repo.Setup(r => r.GetPagedReponseAsync(It.IsAny<GetPositionsQuery>()))
            .ReturnsAsync((new List<Entity> { new() }, recordCount));

        modelHelper.Setup(m => m.ShapeData(It.IsAny<IEnumerable<Entity>>(), It.IsAny<string>() ))
            .Returns(new List<Entity> { new() });

        var handler = new GetAllPositionsQueryHandler(repo.Object, modelHelper.Object);
        var result = await handler.Handle(new GetPositionsQuery { PageNumber = 1, PageSize = 10 }, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(1);
        result.RecordsTotal.Should().Be(2);
    }
}
`
Source: 	ests/MyOnion.Application.Tests/Positions/GetPositionsQueryHandlerTests.cs

This test mirrors the plan’s guidance: mock repository/model helper, call the handler, and assert on the PagedResult metadata.

## Example: Infrastructure Repository Test

`csharp
public class PositionRepositoryAsyncTests
{
    [Fact]
    public async Task AddAsync_PersistsEntity()
    {
        var context = DbContextFactory.Create();
        var repository = new PositionRepositoryAsync(context);
        var position = new Position { Id = Guid.NewGuid(), PositionTitle = "Engineer" };

        await repository.AddAsync(position);
        await context.SaveChangesAsync();

        (await context.Positions.FindAsync(position.Id))
            .Should().NotBeNull();
    }
}
`
Source: 	ests/MyOnion.Infrastructure.Tests/Repositories/PositionRepositoryAsyncTests.cs

InMemory EF Core keeps these tests fast while still exercising real DbContext behavior, as the plan suggests.

## Example: Web API Test

`csharp
public class PositionsControllerTests
{
    [Fact]
    public async Task Get_ReturnsOkWithPagedResult()
    {
        var mediator = new Mock<IMediator>();
        mediator.Setup(m => m.Send(It.IsAny<GetPositionsQuery>(), default))
            .ReturnsAsync(PagedResult<IEnumerable<Entity>>.Success(new List<Entity>(), 1, 10, new RecordsCount()));

        var controller = new PositionsController(mediator.Object);
        var response = await controller.Get(new GetPositionsQuery());

        var okResult = Assert.IsType<OkObjectResult>(response.Result);
        var payload = Assert.IsAssignableFrom<PagedResult<IEnumerable<Entity>>>(okResult.Value);
        payload.IsSuccess.Should().BeTrue();
    }
}
`
Source: 	ests/MyOnion.WebApi.Tests/Controllers/PositionsControllerTests.cs

Even when the test uses a mocked mediator, it confirms that controllers serialize the shared Result contract, reinforcing the plan’s emphasis on API coverage.

## Getting Started

1. **Create the test projects** listed in the plan and reference the production assemblies.
2. **Follow the scope** – ensure handlers, repositories, and controllers each have suites that cover both success and failure paths.
3. **Wire tests into CI** with dotnet test, matching the plan’s deliverables.
4. **Document and revisit** – keep Positions-Unit-Testing.md updated as new scopes arise (e.g., specs or middleware changes).

Adhering to this layered testing plan keeps Template OnionAPI’s Positions feature reliable, and the patterns here translate directly to other aggregates. Copy the structure, adapt the mocks, and you’ll have comprehensive coverage with minimal overhead.
