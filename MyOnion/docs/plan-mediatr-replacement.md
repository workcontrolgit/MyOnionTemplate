# MediatR Replacement Plan

## Goal
Replace MediatR with a free, open source alternative while preserving behavior across requests, handlers, and pipeline behaviors.
Reason: MediatR is moving to a commercial license (see public announcement by Jimmy Bogard).

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

## Progress Review
- Replacement already implemented with an in-house mediator in `MyOnion/src/MyOnion.Application/Messaging/`.
- DI uses `IMediator` from `MyOnion.Application.Messaging` via `MyOnion/src/MyOnion.Application/ServiceExtensions.cs`.
- No `MediatR` package references found in `MyOnion/src` or `MyOnion/tests`.
- FluentValidation is enforced through `ValidationBehavior<,>` in `MyOnion/src/MyOnion.Application/Behaviours/ValidationBehaviour.cs`, registered as a pipeline behavior in `MyOnion/src/MyOnion.Application/ServiceExtensions.cs`. Requests sent via `IMediator.Send(...)` (including create/insert commands) still run validation.

## Status
- [x] Replacement selected: in-house mediator (`MyOnion.Application.Messaging`).
- [x] Inventory completed: handlers and behaviors use local interfaces.
- [x] Dependencies updated: no MediatR packages in `src`/`tests`.
- [x] Handlers updated to in-house mediator interfaces.
- [x] Docs cleanup (README/AGENTS/etc. still mention MediatR).
- [ ] Build/test validation (run `dotnet build` / `dotnet test`).
