# Performance and Security Enhancement Plan

## Goals
- Reduce response latency for dashboard metrics and hot paths.
- Tighten authentication/authorization and runtime security defaults.
- Make risky defaults explicit and configurable.

## Scope
- Dashboard metrics query parallelization.
- Authorization attributes and policy usage.
- CORS defaults and environment guards.
- Sensitive data logging configuration.
- Cache bypass header behavior.

## Task Checklist

### 1) Security Hardening
- [ ] Re-enable `[Authorize]` on `PositionsController` endpoints as intended.
- [ ] Add `[Authorize]` to `EmployeesController`, `DepartmentsController`, and `SalaryRangesController`.
- [ ] Keep `[AllowAnonymous]` only where explicitly intended (e.g., `DashboardController`).
- [ ] Confirm `MetaController` (`/info`) is intended public; if not, add `[Authorize]`.
- [ ] Require explicit origins for CORS in non-development environments.
- [ ] Gate `EnableSensitiveDataLogging()` behind config or environment.
- [ ] Restrict cache bypass header to dev or admin + feature flag.
- [ ] Update docs for security configuration defaults.

### 2) Performance Improvements
- [ ] Parallelize independent dashboard queries in `DashboardMetricsReader`.
- [ ] Validate that concurrent queries do not reuse the same active DbContext.
- [ ] Add a perf regression note or benchmark for dashboard metrics.

### 3) Validation & Rollout
- [ ] Add tests for authorization and CORS behavior.
- [ ] Add tests for cache bypass guardrails.
- [ ] Confirm logs are scrubbed in non-dev settings.
- [ ] Run full test suite and verify no regression.

## Risks
- Parallel query execution may require separate DbContexts if the provider disallows concurrent operations.
- Tightening CORS/auth may impact existing consumers if they relied on permissive defaults.
