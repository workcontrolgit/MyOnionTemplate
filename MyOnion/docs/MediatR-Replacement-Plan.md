# MediatR Replacement Plan

## Goal
Replace MediatR with a free, open source alternative while preserving behavior across requests, handlers, and pipeline behaviors.

## Scope
- Projects: `MyOnion.Application`, `MyOnion.WebApi`, tests.
- Files: DI registration, request/handler definitions, pipeline behaviors, and tests.

## Success Criteria
- All MediatR references removed.
- Replacement mediator integrated with DI and pipeline behaviors.
- Tests updated and passing.

## Work Plan
1. Choose a replacement (e.g., Wolverine, Mediator, or a lightweight in-house mediator).
2. Inventory MediatR usage (requests, handlers, behaviors, DI registration).
3. Update dependencies and DI wiring.
4. Refactor handlers and requests to the new mediator.
5. Update tests and docs.
6. Validate with build and tests.
