# Open-Source Messaging and Mapping in Template OnionAPI v10.1.2

MediatR and AutoMapper are moving to commercial licensing, so Template OnionAPI replaces them with open-source, lightweight alternatives: an in-house mediator (`MyOnion.Application.Messaging`) and Mapster. This keeps the same CQRS + mapping ergonomics while staying free to use in every environment.

Solution Docs: [MediatR-Replacement-Plan](MediatR-Replacement-Plan.md) and [Automapper Replacment with Mapster Plan](Automapper%20Replacment%20with%20Mapster%20Plan.md)

## What the Replacement Does

1. **Lightweight Mediator Core** - `IMediator.Send` resolves handlers by request type, wraps them with pipeline behaviors, and executes them in order. The implementation uses DI to keep handler registration simple.
2. **Pipeline Behaviors Stay Intact** - validation still runs through `IPipelineBehavior<,>` so request validation remains consistent without MediatR.
3. **Mapster Configuration** - `TypeAdapterConfig` scans for `IRegister` mappings (like `GeneralProfile`) and uses `ServiceMapper` for DI-friendly `IMapper`.
4. **Handler Mapping Stays Familiar** - handlers still call `_mapper.Map<TDestination>(source)` so the refactor is low-risk and consistent across command handlers.

## Why the Open-Source Swap Matters

- **Licensing clarity** - no commercial constraints for consumers of the template.
- **Smaller dependency surface** - fewer transitive packages, faster restore, less upgrade churn.
- **Predictable behavior** - mediator + mapping stay under template control, reducing breaking changes.
- **Incremental migration** - APIs look the same to handlers, so diffs stay tight and reviewable.

## Example Code

The in-house mediator is intentionally small: it resolves the handler, wraps pipeline behaviors, and then executes the composed delegate.

```csharp
public Task<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default)
{
    var requestType = request.GetType();
    var handlerType = typeof(IRequestHandler<,>).MakeGenericType(requestType, typeof(TResponse));
    var handler = _serviceProvider.GetRequiredService(handlerType);
    var behaviorsType = typeof(IEnumerable<>).MakeGenericType(
        typeof(IPipelineBehavior<,>).MakeGenericType(requestType, typeof(TResponse)));
    var behaviors = (IEnumerable<object>?)_serviceProvider.GetService(behaviorsType) ?? Array.Empty<object>();

    RequestHandlerDelegate<TResponse> next = () => ((dynamic)handler).Handle((dynamic)request, cancellationToken);
    foreach (var behavior in behaviors.Reverse())
    {
        var current = next;
        next = () => ((dynamic)behavior).Handle((dynamic)request, current, cancellationToken);
    }

    return next();
}
```
Source: `MyOnion/src/MyOnion.Application/Messaging/Mediator.cs:9`

Mapster stays close to AutoMapper ergonomics: mappings are registered once and injected into handlers as `IMapper`.

```csharp
public class GeneralProfile : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<Employee, GetEmployeesViewModel>().TwoWays();
        config.NewConfig<CreateEmployeeCommand, Employee>();
    }
}
```
Source: `MyOnion/src/MyOnion.Application/Mappings/GeneralProfile.cs:9`

```csharp
public async Task<Result<Guid>> Handle(CreateEmployeeCommand request, CancellationToken cancellationToken)
{
    var employee = _mapper.Map<Employee>(request);
    await _repository.AddAsync(employee);
    await _eventDispatcher.PublishAsync(new EmployeeChangedEvent(employee.Id), cancellationToken);
    return Result<Guid>.Success(employee.Id);
}
```
Source: `MyOnion/src/MyOnion.Application/Features/Employees/Commands/CreateEmployee/CreateEmployeeCommand.cs:36`

## Blog Summary

- MediatR is replaced by a lightweight mediator under `MyOnion.Application.Messaging` with pipeline behaviors intact.
- AutoMapper is replaced by Mapster with `TypeAdapterConfig` scanning and `ServiceMapper` injection.
- The change keeps handler code nearly identical while preserving the open-source promise of Template OnionAPI.
