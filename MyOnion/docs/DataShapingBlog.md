# Smarter Data Shaping in Template OnionAPI

Template OnionAPI already supports result shaping through `fields` query parameters, but the current helper leans heavily on reflection. In the .NET 10-supported template release, the data shaping layer has been upgraded so most of the work shifts to cached delegates and EF Core projections, making shaping fast enough for large datasets.

## What the Data Shaping Upgrade Does

1. **Field Parsing Cache** - incoming `fields` strings are normalized (lower-case, trimmed, alphabetical) and cached as `PropertyShapeDescriptor[]`, pairing metadata with compiled accessors. Cache entries expire via LRU + size caps so memory does not grow unbounded.
2. **Compiled Accessors** - each property builds a `Func<T, object?>` via expression trees or `Delegate.CreateDelegate`. Once compiled, shaping a 10k-row collection reuses the delegates instead of calling `PropertyInfo.GetValue` repeatedly.
3. **Database-Level Projection** - repositories (and specifications) accept projection expressions, letting EF Core generate `SELECT FirstName, Title` queries when fields are known. When a request supplies ad-hoc fields, the fallback remains DTO projections that are verified to translate.
4. **No-Op Path + Async Streamlining** - if `fields` is empty, repositories now skip the helper entirely and return the raw DTO/Entity. When shaping is required, `IAsyncEnumerable` surfaces rows as they arrive instead of wrapping synchronous loops inside `Task.Run`.

## Why Smarter Shaping Matters

- **Less CPU per page** - compiled accessors remove reflection hot paths, freeing up thread-pool time for other requests.
- **Smaller result sets** - projections keep EF Core from loading unused columns, reducing I/O and improving cache locality.
- **Predictable async behavior** - no more `Task.Run` wrappers that silently burn threads; shaping can stream results to callers.
- **Configurability + safety** - cache caps and projection fallbacks protect memory, while integration tests cover cache hits/misses and server-side SQL.

## Example Code

FieldShapeCache<T> centralizes normalized field lists and their compiled delegates in one cache so repeat calls simply walk prebuilt accessors instead of invoking reflection for every property on every entity.

```csharp
public sealed record PropertyShapeDescriptor(string Name, Func<object, object?> Accessor);

public sealed class FieldShapeCache<T>
{
    private readonly ConcurrentDictionary<string, (PropertyShapeDescriptor[] Fields, DateTimeOffset LastAccess)> _cache = new();

    public PropertyShapeDescriptor[] GetOrAdd(string? fieldsString)
    {
        var normalized = FieldNormalizer.Normalize(fieldsString);
        return _cache.GetOrAdd(normalized, _ => BuildDescriptors(normalized)).Fields;
    }

    private static PropertyShapeDescriptor[] BuildDescriptors(string normalizedFields)
    {
        var props = string.IsNullOrEmpty(normalizedFields)
            ? typeof(T).GetProperties()
            : normalizedFields.Split(',').Select(name => typeof(T).GetProperty(name, BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance));
        return props
            .Where(p => p is not null)
            .Select(p =>
            {
                var param = Expression.Parameter(typeof(T), "entity");
                var accessor = Expression.Lambda<Func<T, object?>>(
                    Expression.Convert(Expression.Property(param, p!), typeof(object)),
                    param).Compile();
                return new PropertyShapeDescriptor(p!.Name, x => accessor((T)x));
            })
            .ToArray();
    }
}
```
Source: `MyOnion/src/MyOnion.Application/Helpers/DataShapeHelper.cs:1`

The repository example shows how query specifications choose between static projections and dynamic shaping: when the fields parameter is empty it uses typed DTO projections, and when callers request specific fields it builds dynamic selections that fall back to the cached helper if EF Core cannot translate them.

```csharp
public Task<IReadOnlyList<object>> ListAsync(EmployeeSpec spec, string? fields = null, CancellationToken ct = default)
{
    if (string.IsNullOrWhiteSpace(fields))
    {
        return _dbContext.Set<Employee>()
            .WithSpecification(spec)
            .Select(EmployeeListDto.Projection)
            .ToListAsync(ct)
            .ContinueWith(t => t.Result.Cast<object>().ToList(), ct);
    }

    return _dbContext.Set<Employee>()
        .WithSpecification(spec.WithProjection(fields))
        .SelectDynamic(fields)
        .ToListAsync(ct);
}
```
Source: `MyOnion/src/MyOnion.Infrastructure.Persistence/Repositories/EmployeeRepositoryAsync.cs:1`

## Blog Summary

- Data shaping now aligns with the template's .NET 10 support by loading only the fields clients request.
- Cached field descriptors and compiled delegates replace reflection-heavy helpers.
- EF Core projections and no-op bypasses keep the database and API lightweight.
- Async streaming removes the old `Task.Run` overhead while preserving flexible payloads.
