# Folder Restructure Plan

Target structure:
```
├── src/
│   ├── MyOnion.Domain/
│   ├── MyOnion.Application/
│   ├── MyOnion.Infrastructure/
│   └── MyOnion.WebApi/
├── tests/
│   ├── MyOnion.Domain.Tests/
│   ├── MyOnion.Application.Tests/
│   ├── MyOnion.Infrastructure.Tests/
│   └── MyOnion.WebApi.Tests/
└── docs/
```

## Phase 1 – Preparation
1. Inventory current projects and references in `MyOnion.sln`.
2. Note hard-coded file paths in CI scripts, documentation, or project files.
3. Create `src/`, `tests/`, and `docs/` directories alongside the solution.

## Phase 2 – Source Projects (src/)
1. Move `MyOnion.Domain`, `MyOnion.Application`, `MyOnion.Infrastructure.Persistence`, `MyOnion.Infrastructure.Shared`, and `MyOnion.WebApi` into `src/`.
2. Decide whether to merge the persistence/shared projects into a single `MyOnion.Infrastructure` or leave them separate under `src/Infrastructure`.
3. Update each `.csproj` `RootNamespace`/`AssemblyName` only if renames occur (otherwise keep as-is).
4. Edit `MyOnion.sln` to point to new paths; ensure NestedProjects still group correctly.
5. Run `dotnet restore` to validate references.

## Phase 3 – Test Projects (tests/)
1. Move existing test projects into `tests/` (rename folders to `MyOnion.*.Tests`).
2. Update solution entries and any project references to point to `src/…`.
3. Validate `dotnet test` after relocation.

## Phase 4 – Documentation (docs/)
1. Move existing plan/README files that are not code (e.g., result pattern, upgrade plans) into `docs/`.
2. Update Solution Items to reference the new document paths.

## Phase 5 – Cleanup & Validation
1. Remove old directories once the solution builds/tests from the new structure.
2. Update CI/CD scripts (build/test/publish) with new paths.
3. Document the new layout in `README.md` (e.g., “src contains application projects; tests contains automated tests; docs contains plans/guides”).
4. Create a follow-up issue for any remaining path references (docker files, scripts, etc.).

## Risks / Mitigations
- **Broken references:** perform the move in stages, running `dotnet build` after each phase.
- **CI scripts referencing old paths:** search repo/CI configs for `MyOnion.Application` etc., update accordingly.
- **History loss:** use `git mv` to preserve history during file moves.

## Success Criteria
- Solution builds and tests green using the new folder structure.
- Source/test/doc directories match the target layout.
- CI/CD pipelines updated to use new paths.
