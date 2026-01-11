# AutoMapper Replacement with Mapster Plan

## Goal
Replace AutoMapper with Mapster across the solution while keeping API behavior stable and mapping tests reliable.

## Scope
- Projects: `MyOnion.Application`, `MyOnion.Application.Tests`.
- Files: mapping profile(s), DI registration, handler mappings, and test mocks.

## Code Touchpoints (Planned Updates)
- `MyOnion/src/MyOnion.Application/MyOnion.Application.csproj`
  - Remove `AutoMapper` and `AutoMapper.Extensions.Microsoft.DependencyInjection`.
  - Add `Mapster` and `Mapster.DependencyInjection` package references.
- `MyOnion/src/MyOnion.Application/GlobalUsings.cs`
  - Remove `global using AutoMapper;`.
  - Add `global using Mapster;` and `global using MapsterMapper;` as needed.
- `MyOnion/src/MyOnion.Application/ServiceExtensions.cs`
  - Replace `services.AddAutoMapper(Assembly.GetExecutingAssembly());`.
  - Register Mapster config + `IMapper` (Mapster) via `TypeAdapterConfig` and `ServiceMapper`.
- `MyOnion/src/MyOnion.Application/Mappings/GeneralProfile.cs`
  - Convert AutoMapper `Profile` to Mapster `IRegister` and move `CreateMap` calls to `config.NewConfig<TSource, TDest>()`.
- `MyOnion/src/MyOnion.Application/Features/*/Commands/*`
  - Replace injected `AutoMapper.IMapper` with `MapsterMapper.IMapper`.
  - Keep mapping calls via `mapper.Map<T>` or switch to `Adapt<T>` where needed.
- `MyOnion/tests/MyOnion.Application.Tests/GlobalUsings.cs`
  - Replace AutoMapper global using with Mapster.
- `MyOnion/tests/MyOnion.Application.Tests/*/Create*CommandHandlerTests.cs`
  - Replace AutoMapper mocks with Mapster `IMapper` mocks.
- `MyOnion/docs/TestingPlans/Positions-Unit-Testing.md`
  - Update AutoMapper references to Mapster.

## Success Criteria
- All AutoMapper references removed.
- Mapster configuration registered in DI.
- Mapping behavior unchanged for existing DTOs/entities.
- Build and tests pass.

## Decision
- Use Mapster with `TypeAdapterConfig` + `MapsterMapper.IMapper` to keep DI and usage close to the current AutoMapper pattern.
  - Note: Scrutor is already used in `ServiceExtensions.cs` for `IDataShapeHelper` registration; Mapster config scanning can be done via `config.Scan(Assembly)` and does not require Scrutor.

## Rationale (Why Mapster)
- Compatibility: Similar mental model (source -> destination mapping), supports reverse maps, flattening, and custom rules.
- Low-impact migration: `MapsterMapper.IMapper` keeps injection and `Map<T>` usage similar to AutoMapper.
- Performance: Runtime mapping is fast; optional code generation is available if needed.
- OSS-friendly: Free, permissive license, actively maintained.

## Side-by-Side Examples
```csharp
// AutoMapper Profile
public class GeneralProfile : Profile
{
    public GeneralProfile()
    {
        CreateMap<Employee, GetEmployeesViewModel>().ReverseMap();
        CreateMap<CreateEmployeeCommand, Employee>();
    }
}
```

```csharp
// Mapster registration
public class GeneralMapping : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<Employee, GetEmployeesViewModel>()
            .TwoWays();
        config.NewConfig<CreateEmployeeCommand, Employee>();
    }
}
```

```csharp
// AutoMapper usage
public class CreateEmployeeCommandHandler
{
    private readonly IMapper _mapper;
    public CreateEmployeeCommandHandler(IMapper mapper) => _mapper = mapper;

    public Employee Handle(CreateEmployeeCommand command)
        => _mapper.Map<Employee>(command);
}
```

```csharp
// Mapster usage (DI-based mapper)
public class CreateEmployeeCommandHandler
{
    private readonly MapsterMapper.IMapper _mapper;
    public CreateEmployeeCommandHandler(MapsterMapper.IMapper mapper) => _mapper = mapper;

    public Employee Handle(CreateEmployeeCommand command)
        => _mapper.Map<Employee>(command);
}
```

## Work Plan
1. Inventory current mapping usage
   - AutoMapper profile(s) and `IMapper` usage in handlers.
   - AutoMapper package references and global usings.
   - Test mocks and docs references.
2. Add Mapster dependencies and remove AutoMapper
   - Remove AutoMapper packages from `MyOnion.Application`.
   - Add `Mapster` and `Mapster.DependencyInjection`.
3. Replace configuration and DI
   - Convert `GeneralProfile` into Mapster registration via `IRegister`.
   - Replace `AddAutoMapper` registration with Mapster config + `IMapper` registration.
4. Update handler mappings
   - Swap injected `AutoMapper.IMapper` for `MapsterMapper.IMapper`.
   - Keep `Map<T>` usage or update to `Adapt<T>` as needed.
5. Update tests
   - Replace AutoMapper mocks with Mapster `IMapper` mocks.
   - Optionally validate Mapster config in unit tests.
6. Update docs
   - Replace AutoMapper mentions with Mapster (e.g., testing plans).
7. Validate
   - `dotnet build MyOnion.sln`
   - `dotnet test MyOnion.sln`

## Risks & Mitigations
- Mapping behavior drift: add/confirm Mapster config for any custom maps.
- Tests assuming AutoMapper profiles: update tests to use Mapster config or mock.
- DI mismatch: ensure Mapster `IMapper` is registered and injected where needed.

## Checklist
- [ ] Remove AutoMapper packages and usings.
- [ ] Add Mapster packages and config.
- [ ] Convert mapping profiles.
- [ ] Update handlers and tests.
- [ ] Update docs.
- [ ] Build and test.
