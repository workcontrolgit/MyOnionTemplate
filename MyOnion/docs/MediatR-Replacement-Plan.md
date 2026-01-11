# MediatR Replacement Plan - Project "Onion Relay"

## Goal
Replace MediatR with a lightweight in-process mediator while preserving CQRS and pipeline behavior support across the Application layer.

## Scope
- Application layer request/handler abstractions
- Pipeline behaviors (validation, caching, logging, etc.)
- DI registration and handler discovery
- Controllers and tests that depend on `IMediator`
- Remove MediatR package and global usings

## Non-Goals
- Changing request/response DTO shapes
- Introducing external message buses
- Rewriting existing feature logic

## Phases
### Phase 1 - Inventory and Design
- Inventory MediatR usages in Application, WebApi, and tests.
- List current behaviors and handler conventions.
- Define local interfaces (`IRequest<T>`, `IRequestHandler<,>`, `IPipelineBehavior<,>`, `IMediator`).
- Decide on handler registration approach (Scrutor scan or reflection helper).

### Phase 2 - Infrastructure Build
- Implement mediator (compose behaviors and invoke handlers).
- Add DI registration for mediator, handlers, and behaviors.
- Add new namespace/global usings in Application and WebApi.
- Ensure behavior order matches current MediatR setup.

### Phase 3 - Migration
- Update all requests/handlers to new interfaces.
- Replace `MediatR.IMediator` injections with local `IMediator`.
- Update tests and fixtures using MediatR types.
- Remove MediatR package and global usings.

### Phase 4 - Validation and Cleanup
- Build the solution.
- Run tests for Application and WebApi.
- Remove orphaned files/usings and verify analyzers pass.

## Risks and Mitigations
- Behavior order changes could affect validation/caching: mirror existing registration order.
- DI scanning misses handlers: add unit test or startup check to verify counts.
- Runtime resolution issues: keep handler interfaces and constraints consistent.

## Acceptance Criteria
- No references to MediatR packages/usings remain.
- All existing commands/queries work with pipeline behaviors intact.
- Build succeeds and tests pass.

## Validation Commands
- `dotnet build MyOnion.sln -c Release`
- `dotnet test MyOnion.sln`

