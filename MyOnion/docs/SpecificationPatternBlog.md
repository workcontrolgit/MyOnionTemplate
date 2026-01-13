# Harnessing Specifications to Simplify .NET APIs in Template OnionAPI

The Template OnionAPI template leans on the specification pattern to capture query intent in small, reusable objects. Instead of scattering filters, includes, and pagination across repositories, each concern lives in a single spec that any handler can reuse. This post covers what the specification abstraction looks like in the template, why it pays off, and a practical example you can lift into your own .NET apps.

## What the Specification Pattern Looks Like Here

A specification is an object that declares *what* data you need, not *how* to fetch it. The shared interface keeps things consistent:

```csharp
public interface ISpecification<T>
{
    Expression<Func<T, bool>> Criteria { get; }
    List<Expression<Func<T, object>>> Includes { get; }
    List<string> IncludeStrings { get; }
    int? Take { get; }
    int? Skip { get; }
    bool IsPagingEnabled { get; }
    string OrderBy { get; }
}
```
Source: `MyOnion/src/MyOnion.Application/Specifications/ISpecification.cs:1`

`BaseSpecification<T>` wires up helpers for adding includes, applying paging, and setting ordering, while `SpecificationEvaluator` turns any `ISpecification<T>` into a composed EF Core query. Repositories no longer need to know which navigation properties to include or how to apply paging loops—the spec carries that intent.

## Why Use Specifications

- **Less code** – Shared specs eliminate repetitive `if` blocks across handlers and repositories; the filtering rules live in one place.
- **Flexibility** – Each spec exposes paging, ordering, and include knobs, so you can handle exports, dashboards, and filtered lists with the same class by toggling constructor parameters.
- **Testability** – Specifications are just objects; you can unit test their predicates and the evaluator with an in-memory context before wiring them into the API.

These benefits compound over time. Adding a new filter becomes one predicate addition, and multiple consumers stay in sync automatically.

## Example: EmployeesByFiltersSpecification

```csharp
public class EmployeesByFiltersSpecification : BaseSpecification<Employee>
{
    public EmployeesByFiltersSpecification(GetEmployeesQuery request, bool applyPaging = true)
        : base(BuildFilterExpression(request))
    {
        AddInclude(e => e.Position);
        ApplyOrderBy(string.IsNullOrWhiteSpace(request.OrderBy) ? "LastName" : request.OrderBy);

        if (applyPaging && request.PageSize > 0)
        {
            ApplyPaging((request.PageNumber - 1) * request.PageSize, request.PageSize);
        }
    }

    private static Expression<Func<Employee, bool>> BuildFilterExpression(GetEmployeesQuery request)
    {
        var predicate = PredicateBuilder.New<Employee>();

        if (!string.IsNullOrWhiteSpace(request.LastName))
        {
            var term = request.LastName.ToLower().Trim();
            predicate = predicate.Or(p => p.LastName.ToLower().Contains(term));
        }

        if (!string.IsNullOrWhiteSpace(request.FirstName))
        {
            var term = request.FirstName.ToLower().Trim();
            predicate = predicate.Or(p => p.FirstName.ToLower().Contains(term));
        }

        if (!string.IsNullOrWhiteSpace(request.Email))
        {
            var term = request.Email.ToLower().Trim();
            predicate = predicate.Or(p => p.Email.ToLower().Contains(term));
        }

        if (!string.IsNullOrWhiteSpace(request.EmployeeNumber))
        {
            var term = request.EmployeeNumber.ToLower().Trim();
            predicate = predicate.Or(p => p.EmployeeNumber.ToLower().Contains(term));
        }

        if (!string.IsNullOrWhiteSpace(request.PositionTitle))
        {
            var term = request.PositionTitle.ToLower().Trim();
            predicate = predicate.Or(p => p.Position.PositionTitle.ToLower().Contains(term));
        }

        return predicate.IsStarted ? predicate : null;
    }
}
```
Source: `MyOnion/src/MyOnion.Application/Specifications/Employees/EmployeeSpecifications.cs:1`

Highlights from this spec:

- The constructor decides whether to apply paging; you can reuse the same spec for paged lists or full exports.
- Includes are tied to the spec, so repositories do not need to remember to eagerly load `Position`.
- The predicate is composable, making it trivial to add or remove filters without touching consumers.

## Where the Spec Runs

Repositories expose spec-aware methods so handlers simply pass the description of the data they want:

```csharp
public async Task<IReadOnlyList<T>> ListAsync(ISpecification<T> specification)
{
    var queryable = SpecificationEvaluator<T>.GetQuery(
        _dbContext.Set<T>().AsQueryable(),
        specification);

    return await queryable.ToListAsync(cancellationToken);
}
```

Because specifications are immutable, multiple handlers can share the same instance safely, and you can swap specs at runtime based on user input (keyword search vs. filter panel) without changing repository code.

## Getting Started

1. **Define a spec contract** just like `ISpecification<T>` so every query exposes the same knobs.
2. **Build a base class** with helpers to add includes, paging, and ordering; keep the API expressive but focused.
3. **Create a spec catalogue** per aggregate—`EmployeesByFiltersSpecification`, etc.—to capture real scenarios.
4. **Expose spec-friendly repository methods** such as `ListAsync(ISpecification<T>)` or `FirstOrDefaultAsync(ISpecification<T>)` so consumers pass intent instead of ad-hoc predicates.

Once you capture filtering logic inside specs, you spend less time wiring boilerplate queries, gain flexibility when requirements change, and end up with small objects that are easy to test in isolation. That combination is why the specification pattern has become the backbone of data access in Template OnionAPI.

