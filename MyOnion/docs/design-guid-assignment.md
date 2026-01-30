# GUID Assignment Technical Approach

## Goal
Ensure every new entity created through the API has a non-empty GUID, with consistent behavior across all write paths.

## Current Behavior
- Entity IDs are configured as `ValueGeneratedNever`, so the database will not generate GUIDs.
- `CreateEmployeeCommand` explicitly sets `Guid.NewGuid()`, while other create handlers rely on the mapper and do not set `Id`.

## Options
1. **Assign in every create handler**
   - **Pros:** Explicit and local; easy to reason about per command.
   - **Cons:** Easy to miss a handler; duplicates logic across features; inconsistent if any handler is skipped.

2. **Assign in domain constructors**
   - **Pros:** Centralized in the entity type; new entities always get IDs.
   - **Cons:** EF Core materialization can overwrite or bypass; unit tests and mappers may create entities with unintended IDs; reduces control for imports/seeding.

3. **Assign in mapper configuration**
   - **Pros:** Keeps assignment near mapping logic.
   - **Cons:** Still fragmented by mapper profiles; easy to forget for new mappings; hard to enforce globally.

4. **Assign in `ApplicationDbContext.SaveChanges` / `SaveChangesAsync`**
   - **Pros:** Single, consistent enforcement for all entities; works for all create paths (handlers, seeds, tests); avoids duplication.
   - **Cons:** Must ensure it only runs for added entities and only when `Id == Guid.Empty`.

5. **Database default (`NEWID()` / `NEWSEQUENTIALID()`)**
   - **Pros:** Guarantees IDs at the database layer.
   - **Cons:** Conflicts with `ValueGeneratedNever`; requires schema change and migrations; still leaves code without IDs until save.

## Selected Approach: ApplicationDbContext
Use a centralized `SaveChanges`/`SaveChangesAsync` hook in `ApplicationDbContext` to assign IDs for added entities that inherit `BaseEntity` and have `Id == Guid.Empty`.

**Why this approach**
- Guarantees consistency without touching every handler.
- Keeps the existing `ValueGeneratedNever` model configuration intact.
- Works for all entry points (commands, seeds, tests, background jobs).
- Easy to test: added entities get IDs before save completes.

## Implementation Outline
1. Override `SaveChanges` and `SaveChangesAsync` in `ApplicationDbContext`.
2. Before calling base save, loop through `ChangeTracker.Entries<BaseEntity>()` where `State == Added`.
3. If `Id == Guid.Empty`, assign `Guid.NewGuid()`.
4. Keep `ValueGeneratedNever` configuration to avoid database-generated IDs.

Sample code:
```csharp
public override int SaveChanges()
{
    AssignIds();
    return base.SaveChanges();
}

public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
{
    AssignIds();
    return base.SaveChangesAsync(cancellationToken);
}

private void AssignIds()
{
    foreach (var entry in ChangeTracker.Entries<BaseEntity>()
        .Where(e => e.State == EntityState.Added))
    {
        if (entry.Entity.Id == Guid.Empty)
        {
            entry.Entity.Id = Guid.NewGuid();
        }
    }
}
```

## Notes
- For imports or deterministic IDs, callers can still set `Id` explicitly; the hook should not overwrite non-empty values.
- If sequential GUIDs are preferred later, the ID assignment method can be swapped to a sequential generator in one place.
