# Data Shaping Upgrade Plan

## Current Pain Points
- Reflection-heavy `DataShapeHelper<T>` parses requested fields and calls `PropertyInfo.GetValue` for every entity/property combination, which becomes the hot path on large result sets.
- Repositories always retrieve full entity graphs from EF Core and only trim data after materialization, so the database sends unnecessary columns.
- `ShapeDataAsync` simply wraps the synchronous logic in `Task.Run`, consuming thread-pool threads without reducing CPU usage.
- When no `fields` parameter is provided, the helper still duplicates every property into a new `Entity` dictionary, resulting in extra allocations.

## Recommended Improvements
1. **Field Parsing Cache**
   - Normalize `fieldsString` (e.g., comma-separated lower-case list) and cache the resulting `PropertyInfo[]`.
   - Evict rarely used entries via an LRU or size cap to avoid unbounded growth.

2. **Compiled Accessors**
   - Pre-build `Func<T, object?>` accessors for each property using expression trees or `Delegate.CreateDelegate`, avoiding repeated reflection per entity.
   - Store accessors alongside cached field lists for quick reuse.

3. **Database-Level Projection**
   - Extend specifications or repositories to accept projection expressions so EF Core performs `Select` with only requested columns.
   - When dynamic fields prevent compile-time expressions, fall back to static DTO projections for common views (e.g., employee list, position list).

4. **No-Op for Full Payloads**
   - Short-circuit shaping when `fieldsString` is null/whitespace by returning the original entities or by cloning references only when needed.

5. **Async Streamlining**
   - Remove `Task.Run`-based `ShapeDataAsync`; if asynchronous processing is required, leverage `IAsyncEnumerable` and yield shaped rows as they arrive.

## Rollout Steps
1. Introduce caching + compiled accessors in `DataShapeHelper<T>` behind an interface-preserving refactor.
2. Update repositories to bypass shaping when `fields` is empty; add integration tests covering both cached and cache-miss paths.
3. Enhance specifications to supply projection expressions; verify EF SQL shows trimmed column lists.
4. Measure before/after using realistic datasets (10k+ rows) and capture CPU/time metrics.
5. Document helper usage patterns and add guidelines for future query handlers in the README/architecture docs.

## Risks & Mitigations
- **Cache Memory Growth:** enforce maximum cache entries and expose metrics to monitor hit rates.
- **Breaking Dynamic Queries:** ensure projection changes fall back to current behavior when fields cannot be translated server-side.
- **Testing Complexity:** add unit tests around helper caching and integration tests covering both synchronous and asynchronous flows.
