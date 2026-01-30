# Migrating Specifications to Ardalis.Specification in Template OnionAPI v10.2.0

Template OnionAPI v10.2.0 updates the specification pattern to use Ardalis.Specification. The goal is simple: remove custom infrastructure, gain type-safe includes and ordering, and unlock EF Core features like split queries and query tags. This post summarizes the upgrade plan, why it matters, and what the migration looks like in practice.

## What Changes in v10.2.0

The existing solution uses a custom `BaseSpecification<T>` and a `SpecificationEvaluator` that rely on string-based ordering and string includes for nested navigation. The upgrade replaces those with Ardalis.Specification's fluent API.

Key shifts:

- Specification base classes come from Ardalis.Specification
- Includes and ordering are expression-based and type-safe
- Ordering supports `OrderByDescending` and `ThenBy`
- Per-specification query options become first-class: `AsNoTracking`, `AsSplitQuery`, `TagWith`, and more
- Custom infrastructure is removed (roughly 280 lines)

## Why the Migration Is Worth It

The upgrade plan targets the problems the current implementation struggles with:

- String-based ordering has runtime-only validation and no IntelliSense
- Nested includes rely on string paths that break on refactors
- Sorting is limited to single-field ascending order
- There is no way to define query features like split queries or tracking behavior per specification

Ardalis.Specification addresses each of these without changing how handlers or repositories are used at a high level.

## Example: Old vs New Specification

Below is a simplified comparison from the upgrade plan.

### Old (custom BaseSpecification)

```csharp
public class EmployeesByFiltersSpecification : BaseSpecification<Employee>
{
    public EmployeesByFiltersSpecification(GetEmployeesQuery request, bool applyPaging = true)
        : base(BuildFilterExpression(request))
    {
        AddInclude(e => e.Position);
        AddInclude("Position.Department");
        ApplyOrderBy(request.OrderBy ?? "LastName");

        if (applyPaging && request.PageSize > 0)
            ApplyPaging((request.PageNumber - 1) * request.PageSize, request.PageSize);
    }
}
```

### New (Ardalis.Specification)

```csharp
public class EmployeesByFiltersSpec : Specification<Employee>
{
    public EmployeesByFiltersSpec(GetEmployeesQuery request, bool applyPaging = true)
    {
        if (!string.IsNullOrWhiteSpace(request.LastName))
            Query.Where(e => e.Name.LastName.Contains(request.LastName));

        Query.Include(e => e.Position)
             .ThenInclude(p => p.Department);

        Query.OrderBy(e => e.Name.LastName)
             .ThenBy(e => e.Name.FirstName);

        if (applyPaging && request.PageSize > 0)
            Query.Skip((request.PageNumber - 1) * request.PageSize)
                 .Take(request.PageSize);

        Query.AsNoTracking()
             .AsSplitQuery()
             .TagWith("GetEmployeesByFilters");
    }
}
```

The new approach removes string ordering and string includes, adds multi-field ordering, and makes EF Core query options part of the specification itself.

## Repository Integration

The repository only swaps the evaluator. Everything else stays intact.

```csharp
using Ardalis.Specification;
using Ardalis.Specification.EntityFrameworkCore;

public async Task<IReadOnlyList<T>> ListAsync(ISpecification<T> specification)
{
    return await SpecificationEvaluator.Default
        .GetQuery(_dbSet.AsQueryable(), specification)
        .ToListAsync();
}
```

This is a small change with a big payoff: no more custom evaluator maintenance, and specifications control tracking and query behavior directly.

## Migration Plan at a Glance

The upgrade plan breaks the work into a few predictable steps:

1. Add Ardalis.Specification packages.
2. Create new specs (start with a low-risk entity like Department).
3. Update repository interfaces for projection support.
4. Migrate query handlers and specs incrementally.
5. Remove old specification infrastructure and unused dependencies.
6. Run tests and validate SQL output.

The document estimates 8-14 hours total and calls out rollback triggers and validation steps so the migration stays safe and reversible.

## Testing Focus

The testing plan is practical and layered:

- Unit tests per specification
- Integration tests for repository + database behavior
- SQL query comparison for the most important specs
- API-level tests to ensure endpoints return identical results

This lets the team prove the migration is behavior-preserving while still adopting the improved specification API.

## What You Get in v10.2.0

By moving to Ardalis.Specification, Template OnionAPI v10.2.0 delivers:

- Type-safe includes and ordering
- Multi-field sorting and descending ordering
- Per-spec query tuning with EF Core features
- Cleaner repository code
- Less infrastructure to maintain

If you are maintaining a custom specification pattern, this upgrade is a clean, low-risk way to modernize without changing your domain or application layer architecture.

---

Source: `MyOnion/docs/Ardalis_Specification_Upgrade_Plan.md`
