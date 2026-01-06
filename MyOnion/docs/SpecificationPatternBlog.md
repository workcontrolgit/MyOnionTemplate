# Harnessing Specifications to Simplify .NET APIs in Template OnionAPI

MyOnion’s Specification Upgrade Plan doesn’t just modernize repository code—it formalizes a lightweight pattern that any .NET developer can lift into their own solutions. This post walks through what the specification pattern is inside the template, why it matters, and how you can start using it immediately.

## What the Specification Pattern Is Here

At its core, a specification is a small, reusable object that describes *what* you want from the data store rather than *how* to get it. The template ships with a simple contract:

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

`BaseSpecification<T>` implements the plumbing—tracking criteria, includes, paging, and ordering—while `SpecificationEvaluator` applies those pieces to an EF Core `IQueryable`. The evaluator simply checks each property and builds up the final query, which keeps the repositories almost declarative.

## Why the Template Adopted It

SpecificationUpgradePlan.md highlights a few motivators:

- **Centralized query intent** – Filtering, ordering, paging, and eager-loading all live in specs, so repositories stop bloating with `if/else` ladders and controllers don’t duplicate logic.
- **Safer refactors** – Generic repositories can accept `ISpecification<T>` and defer execution to EF Core. Because the spec owns the logic, merging new filters or includes is far less risky.
- **Documentation-as-code** – Having named specs like `EmployeesByFiltersSpec` doubles as living documentation that reflects the “catalogue” described in Phase 4 of the plan.
- **Extensibility** – When the plan later adds Department or Salary specs, teams follow the same recipe: extend `BaseSpecification<T>`, compose predicates with `PredicateBuilder`, and plug into the same evaluator.

All of this keeps data-shaping helpers (`IDataShapeHelper`, `IModelHelper`) intact: specs feed clean, materialized sets into the existing dynamic projection pipeline.

## Example: Filtering Employees with One Spec

Here’s a trimmed version of the employee filter spec you’ll find in the template:

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

        // Additional FirstName, Email, EmployeeNumber, PositionTitle filters removed for brevity...

        return predicate.IsStarted ? predicate : null;
    }
}
```
Source: `MyOnion/src/MyOnion.Application/Specifications/Employees/EmployeeSpecifications.cs:1`

A few tips from this example:

- Criteria are composed with `PredicateBuilder`, so adding another filter is a one-line expression.
- Includes (`AddInclude(e => e.Position)`) keep eager loading coupled to the spec, not the repository.
- Ordering and paging remain optional knobs—handy when the same spec powers both paged lists and exports.

## Where It Gets Consumed

The generic repository only needs one extra method:

```csharp
public async Task<IReadOnlyList<T>> ListAsync(ISpecification<T> specification)
{
    var queryable = SpecificationEvaluator<T>.GetQuery(_dbContext.Set<T>().AsQueryable(), specification);
    return await queryable.ToListAsync(cancellationToken);
}
```

Because specs are immutable descriptions, handlers can swap them freely. For instance, an API endpoint that supports free-text search can take a `PagedEmployeesQuery`, hydrate the `EmployeesKeywordSpecification`, and pass it straight to `ListAsync`. Dynamic shaping still happens afterward, but *every* consumer benefits from the same, battle-tested query logic.

## Getting Started in Your Own Code

1. **Define your spec contract** (or reuse this one) with criteria, includes, ordering, and paging knobs.
2. **Implement `BaseSpecification<T>` and a simple evaluator** like the template’s to compose EF Core queries.
3. **Build a small specification catalogue** per aggregate, as outlined in the plan’s Phase 4. Treat each spec like a scenario-focused query object.
4. **Gradually refactor repositories and handlers** to call spec-aware methods. Keep old signatures during migration, exactly as the plan suggests, so you can roll out safely.

Once in place, the specification pattern becomes the single source of truth for your data-access intent. In MyOnion it unlocked clearer repositories, easier testing (you can unit test specs and evaluator separately), and better documentation. Adopt the same approach in your .NET projects, and the payoff is cleaner code and faster iteration every time a product manager asks for “just one more filter.”
