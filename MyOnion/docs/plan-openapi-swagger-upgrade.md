# OpenAPI & Swagger UI Upgrade Plan

This document tracks the work required to drop Swashbuckle while keeping Swagger UI backed by an OpenAPI document.

## 1. Decide on Replacement
- Adopt **NSwag.AspNetCore** to generate OpenAPI specs and host Swagger UI. It supports .NET 10 and removes the `Microsoft.OpenApi` dependency that blocked Swashbuckle.
- Verify parity: security schemes, versioning, XML comments, and custom filters must map to NSwag equivalents.

## 2. Prototype Integration
1. Add packages (`NSwag.AspNetCore`, `NJsonSchema`) to `MyOnion.WebApi`.
2. Replace `AddSwaggerExtension` registration with `AddOpenApiDocument` configuration:
   - Set title/description/contact.
   - Configure JWT security definition via NSwag's fluent API.
   - Hook Asp.Versioning explorer if needed.
3. Update middleware: swap `UseSwaggerExtension` with `app.UseOpenApi()` + `app.UseSwaggerUi3()`.
4. Remove Swashbuckle packages from the project file.

## 3. Testing & Validation
- Run `dotnet build` to confirm dependency cleanup.
- Hit `/swagger/v1/swagger.json` and validate schema sections (paths, security, metadata).
- Open Swagger UI, confirm endpoints render and the Authorize button works.
- Compare JSON diff vs. the legacy spec for breaking changes.

## 4. Documentation & Cleanup
- Update README/internal docs explaining NSwag setup (e.g., how to regenerate clients if needed).
- Remove unused Swashbuckle extension classes/config.
- Note how to customize NSwag filters for future work.

## 5. Rollout
- Merge once QA signs off on UI parity.
- Monitor API deployments for any missing annotations or doc regressions.

**Success criteria:** Swashbuckle packages removed, NSwag drives the same Swagger UI experience, and the OpenAPI JSON remains accurate for client generation.

## Current Implementation Status
- `MyOnion.WebApi` now references `NSwag.AspNetCore` instead of Swashbuckle/Microsoft.OpenApi.
- `AddSwaggerExtension` registers `AddOpenApiDocument` with JWT security, and middleware serves docs via `UseOpenApi`/`UseSwaggerUi3`.
- Build verified via `dotnet build MyOnion.sln`; next validate Swagger UI + JSON responses in a running environment.
