# EasyCaching Migration Plan

## Goals
- Improve cache performance and reduce cloud cost.
- Keep flexible invalidation when backend data changes.
- Preserve cache diagnostics headers: `X-Cache-Status`, `X-Cache-Key`, `X-Cache-Duration-Ms`.
- Keep hash vs raw cache key display configurable, and allow invalidation with either.

## Current Usage Inventory
- Pipeline caching behaviors for:
  - Employees: `GetEmployeesCachingDecorator`
  - Positions: `GetPositionsCachingBehavior`
- Invalidation:
  - Admin endpoint `POST /api/v1/cache/invalidate`
  - Command handlers invalidate employees cache prefix on create/update/delete
- Diagnostics headers:
  - Configurable in `Caching:Diagnostics`
  - Supports hash vs raw key display

## Target Design
### Configuration (WebApi)
- Add EasyCaching packages (Memory + Redis).
- Configure in `Program.cs`:
  - `AddEasyCaching(options => ...)`
  - Choose providers via appsettings: memory only, redis only, or hybrid.
- Keep `Caching:Diagnostics` settings to control headers and key display mode.

### Caching API
- Wrap EasyCaching with a small adapter to match current usage:
  - `GetAsync<T>(key)`
  - `SetAsync<T>(key, ttl)`
  - `RemoveAsync(key)`
  - `RemoveByPrefixAsync(prefix)` or `RemoveByTagAsync(tag)`
- Prefer **tags** for invalidation:
  - Tag all Employees responses with `Employees:GetAll`
  - Tag all Positions responses with `Positions:GetAll`

### Diagnostics Headers
- Keep a thin diagnostics publisher:
  - Emits `X-Cache-Status`, `X-Cache-Key`, `X-Cache-Duration-Ms`.
  - Uses `Caching:Diagnostics` settings.
  - Supports `KeyDisplayMode: Raw|Hash` with hash invalidation mapping.

## Implementation Steps
1) Add EasyCaching packages to WebApi (and Redis provider if distributed).
2) Add EasyCaching configuration section to appsettings:
   - Provider selection
   - Redis connection string
3) Build an adapter around EasyCaching:
   - Map existing cache keys and prefixes/tags
   - Track keys for hashed invalidation
4) Replace current caching behaviors with EasyCaching adapter calls.
5) Update invalidation endpoint to use tags/prefixes and hash mapping.
6) Remove custom caching project/code and references.
7) Update docs/tests to match new behavior.

## Verification
- Hit `GET /api/v1/Employees` twice: expect MISS then HIT.
- Confirm headers match config names.
- Invalidate by tag/prefix and verify next request is MISS.
- Test hash invalidation when `KeyDisplayMode=Hash`.

