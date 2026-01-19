# Docker Support Plan

## Objectives
- Provide a repeatable, containerized workflow for local development and CI.
- Ship a production-ready Docker image for `MyOnion.WebApi`.
- Define runtime configuration and secrets handling for containers.

## Scope
1. **Container Images**
   - Add a multi-stage `Dockerfile` for `MyOnion.WebApi` targeting net10.0.
   - Use a slim runtime image and non-root user where possible.

2. **Local Orchestration**
   - Add a `docker-compose.yml` for the API and required dependencies (SQL Server or other configured provider).
   - Map required ports and mount dev-only volumes when needed.
   - Preserve the current host URL by mapping container HTTPS `8443` to `https://localhost:44378`.

3. **Configuration & Secrets**
   - Define environment variables for `Sts:*`, `Cors:*`, connection strings, and logging.
   - Provide `.env.example` or compose overrides for local secrets (no real secrets committed).

4. **Operational Concerns**
   - Add health checks to containers.
   - Document how to run migrations and seed data in containerized flows.
   - Provide a dev-cert approach that mounts a local dev cert into the container for HTTPS.

5. **Documentation**
   - Add usage instructions to `README.md` or a new Docker doc in `docs/`.

## Implementation Steps
1. **Discovery**
   - Confirm required services (database, STS dependency, external APIs).
   - Decide whether to include optional services in compose or require external endpoints.

2. **Dockerfile**
   - Implement multi-stage build (restore/build/publish).
   - Set runtime environment variables and expose the API port.

3. **Compose Setup**
   - Add `docker-compose.yml` and `docker-compose.override.yml` for dev defaults.
   - Add volumes for persisted data and configure container networking.
   - Map HTTPS port `44378` on the host to container HTTPS port `8443`.

4. **Configuration**
   - Add container-friendly settings in `appsettings.Development.json` (if needed).
   - Document required environment variables and defaults.
   - Document host cert export, mount paths, and `ASPNETCORE_Kestrel__Certificates__Default__*` env vars.
   - Add a step to create/export the dev certificate for host mounting.
   - Use these commands to create and export the dev cert:
     ```powershell
     $certPath = "$env:USERPROFILE\\.aspnet\\https\\myonion-dev.pfx"
     $certPassword = "devpassword"
     dotnet dev-certs https --clean
     dotnet dev-certs https --trust
     dotnet dev-certs https -ep $certPath -p $certPassword
     ```

5. **Validation**
   - Verify `docker compose up` builds and serves the API.
   - Validate health endpoints, logs, and configuration overrides.

## Risks & Mitigations
- **Image Size Growth:** Use multi-stage builds and trim publish outputs.
- **Secrets Leakage:** Keep secrets in `.env` and user-secrets, never in source.
- **Environment Drift:** Document required variables and provide examples.
