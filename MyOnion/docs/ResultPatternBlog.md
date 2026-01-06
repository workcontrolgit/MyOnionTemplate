# Building Reliable APIs with the Result Pattern in Template OnionAPI

Template OnionAPI standardizes every command and query around a simple Result pattern. Instead of returning loosely coupled DTOs or throwing exceptions up the stack, handlers describe whether an operation succeeded, what payload it produced, and which errors occurred. This post breaks down what the Result abstraction looks like, why it pays off for developers, and how it shows up in everyday code.

## What the Result Pattern Looks Like

The Application layer defines a pair of types—`Result` and `Result<T>`—that capture the outcome of any operation:

```csharp
public class Result
{
    protected Result(bool isSuccess, string message, IReadOnlyCollection<string> errors)
    {
        IsSuccess = isSuccess;
        Message = message;
        Errors = errors ?? Array.Empty<string>();
    }

    public bool IsSuccess { get; }
    public bool IsFailure => !IsSuccess;
    public string Message { get; }
    public IReadOnlyCollection<string> Errors { get; }
    public double? ExecutionTimeMs { get; private set; }

    public static Result Success(string message = null) => new(true, message, Array.Empty<string>());
    public static Result Failure(string message, IEnumerable<string> errors = null)
    {
        var errorList = BuildErrors(message, errors);
        return new Result(false, message, errorList);
    }
}

public class Result<T> : Result
{
    protected Result(bool isSuccess, T value, string message, IReadOnlyCollection<string> errors)
        : base(isSuccess, message, errors)
    {
        Value = value;
    }

    public T Value { get; }

    public static Result<T> Success(T value, string message = null)
        => new(true, value, message, Array.Empty<string>());

    public new static Result<T> Failure(string message, IEnumerable<string> errors = null)
    {
        var errorList = BuildErrors(message, errors);
        return new Result<T>(false, default!, message, errorList);
    }
}
```
Source: `MyOnion/src/MyOnion.Application/Common/Results/Result.cs:1`

Extensions like `PagedResult<T>` and `PagedDataTableResult<T>` inherit from `Result<T>` so paged responses share the exact same success/failure semantics while adding pagination metadata.

## Why Use Result Objects

- **Consistent contracts** – Every handler returns a predictable JSON shape (`isSuccess`, `message`, `errors`, `value`), so controllers, middleware, and front-end clients no longer branch on ad-hoc types.
- **Less error-handling boilerplate** – Callers can check `IsFailure` without catching exceptions for expected states such as validation issues or missing entities.
- **Built-in diagnostics** – Properties like `ExecutionTimeMs` allow filters/middleware to attach metrics to the response object itself.
- **Testability** – Because Results are plain objects, you can unit test handlers or middleware by asserting on a single type rather than mocking HTTP responses.

## Example: A Handler Returning `Result<T>`

```csharp
public class GetPositionByIdQuery : IRequest<Result<Position>>
{
    public Guid Id { get; set; }

    public class GetPositionByIdQueryHandler : IRequestHandler<GetPositionByIdQuery, Result<Position>>
    {
        private readonly IPositionRepositoryAsync _positionRepository;

        public async Task<Result<Position>> Handle(GetPositionByIdQuery query, CancellationToken cancellationToken)
        {
            var entity = await _positionRepository.GetByIdAsync(query.Id);
            if (entity == null)
            {
                return Result<Position>.Failure("Position not found");
            }

            return Result<Position>.Success(entity);
        }
    }
}
```
Source: `MyOnion/src/MyOnion.Application/Features/Positions/Queries/GetPositionById/GetPositionByIdQuery.cs:4`

Handlers follow a simple pattern: fetch data, return `Result<T>.Success` when a value exists, or `Result<T>.Failure` with a helpful message otherwise. Commands such as `UpdatePositionCommand` behave the same way, which keeps controller actions thin.

## Example: Paged Responses with the Result Contract

```csharp
public async Task<PagedResult<IEnumerable<Entity>>> Handle(GetPositionsQuery request, CancellationToken cancellationToken)
{
    var result = await _positionRepository.GetPagedReponseAsync(objRequest);
    var shapedData = _dataShapeHelper.ShapeData(result.data, request.Fields);

    return PagedResult<IEnumerable<Entity>>.Success(
        shapedData,
        request.PageNumber,
        request.PageSize,
        result.recordsCount);
}
```
Source: `MyOnion/src/MyOnion.Application/Features/Positions/Queries/GetPositions/GetPositionsQuery.cs:50`

The paged variant still exposes `IsSuccess`, `Message`, and `Errors`, but it also reports pagination metadata (`PageNumber`, `PageSize`, `RecordsFiltered`, `RecordsTotal`). Consumers never have to guess how paging data is shaped.

## Getting Started with the Result Pattern

1. **Return `Result` everywhere** – Update your MediatR handlers (commands and queries) to return `Result`/`Result<T>`/`PagedResult<T>` instead of bare DTOs.
2. **Push logic to middleware** – Centralize exception handling and validation errors by translating them into `Result.Failure` within middleware like `ErrorHandlerMiddleware`.
3. **Expose Results over HTTP** – Controllers can simply `return Ok(result);` because the response already contains both metadata and payload.
4. **Test outcomes, not transports** – Assert directly on `Result` instances in unit tests to verify success paths, error messages, and pagination counts.

Adopting the Result pattern gives Template OnionAPI a single vocabulary for success and failure. Developers write less repetitive error-handling code, clients enjoy uniform responses, and tests can focus on behavior instead of HTTP plumbing. Start small—wrap one handler at a time—and you’ll quickly feel the benefits across the entire stack.
