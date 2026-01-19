# Endpoint Timing Plan

## Objectives
- Capture a precise elapsed time for every API endpoint execution.
- Surface per-request timing information in responses and Result payloads.
- Provide a structured approach for implementation, review, and rollout with minimal performance overhead.

## Scope
1. **Request Timing Middleware**
   - Build ASP.NET Core middleware that records `Stopwatch` start/end around the request pipeline.
   - Store elapsed milliseconds in `HttpContext.Items` for downstream consumers.
   - Ensure compatibility with existing error handling and logging middleware.

2. **API Response Augmentation**
   - For JSON APIs, include the elapsed time (e.g., `x-execution-time-ms`) as a response header.
   - Extend `Result` and `PagedResult` with an `ExecutionTimeMs` field; populate it from the timing middleware before serialization so clients see the metric in every Result payload.

3. **Configuration & Observability**
   - Provide configuration flags to enable/disable timing and header injection per environment.
   - Emit structured logs (with endpoint name + elapsed time) for later analysis.

4. **Testing & Validation**
   - Unit tests for middleware to ensure elapsed time is calculated and headers are set.
   - Performance validation to ensure minimal overhead (<1ms median) under typical loads.

## Implementation Steps
1. **Design Review**
   - Present this plan for approval.
   - Validate scope with stakeholders (API + front-end teams).

2. **Middleware Development**
   - Implement timing middleware and register it early in the pipeline.
   - Add configuration options (appsettings + DI binding).

3. **Instrumentation & Docs**
   - Extend logging to include endpoint timing.
   - Document configuration and usage in README/internal wiki.

4. **Testing & Rollout**
   - Run automated tests.
   - Perform staged deployment with monitoring to ensure no regressions.

## Risks & Mitigations
- **Performance Impact:** Mitigate via lightweight Stopwatch usage and benchmarking.
- **Configuration Drift:** Centralize settings and default to safe (disabled) behavior if misconfigured.
