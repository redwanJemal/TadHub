# TadHub - Tadbeer ERP Platform

**UAE Domestic Worker Recruitment Lifecycle Management**

11 Modules | 3 Contract Types | 19 Job Categories | 20+ Worker States

Compliant with:
- Federal Decree-Law No. 9 of 2022
- MoHRE Standards
- WPS 2025

## Overview

TadHub is a comprehensive ERP platform for Tadbeer centers (UAE domestic worker recruitment agencies). Built on a modular .NET 9 architecture with multi-tenancy, event-driven design, and full regulatory compliance.

## Tech Stack

- **.NET 9** - Backend API
- **PostgreSQL** - Primary database with RLS
- **Redis** - Caching & distributed locks
- **RabbitMQ + MassTransit** - Event bus
- **Keycloak** - Identity & access management
- **Hangfire** - Background jobs
- **Meilisearch** - Full-text search
- **MinIO** - Object storage

## Modules

| Module | Description | Status |
|--------|-------------|--------|
| **Tenant & Agency** | Multi-tenancy with Tadbeer licensing, shared pool agreements | 🔄 Adapting |
| **IAM** | Domain roles (Receptionist, Cashier, PRO, Agent), permissions | 🔄 Adapting |
| **Client Management** | Employer lifecycle, verification, lead tracking | ⏳ Pending |
| **Worker/CV Management** | 20-state machine, CV, job categories, passport custody | ⏳ Pending |
| **Contract Engine** | Traditional/Temporary/Flexible contracts, guarantees, refunds | ⏳ Pending |
| **Financial & Billing** | Invoicing, VAT, milestones, X-Reports | ⏳ Pending |
| **PRO & Govt Gateway** | 8 transaction types, visa, medical, Emirates ID | ⏳ Pending |
| **Scheduling** | Flexible bookings, labor law enforcement | ⏳ Pending |
| **WPS** | Payroll, SIF generation, compliance | ⏳ Pending |
| **Notifications** | WhatsApp, SMS, bilingual templates, escalation | ⏳ Pending |
| **Reporting** | Role-based dashboards, MoHRE compliance reports | ⏳ Pending |

## Contract Types

1. **Traditional** - 2-year, employer sponsorship, 180-day guarantee
2. **Temporary** - 6-month agency sponsorship, transferable
3. **Flexible** - Variable duration (4h-12mo), per-unit pricing

## Project Structure

```
src/
├── TadHub.Api/              # REST API + controllers
├── TadHub.Infrastructure/   # EF Core, caching, messaging, auth
├── TadHub.SharedKernel/     # Base entities, events, interfaces
└── Modules/
    ├── Tenancy/             # Multi-tenant foundation
    ├── Identity/            # User profiles
    ├── Authorization/       # Roles & permissions
    ├── Notification/        # Multi-channel notifications
    ├── Analytics/           # Usage tracking
    └── Tadbeer/             # Domain modules (to be added)
        ├── ClientManagement/
        ├── Worker/
        ├── Contract/
        ├── Financial/
        ├── ProGateway/
        ├── Scheduling/
        ├── Wps/
        └── Reporting/
```

## Getting Started

See `docker/README.md` for full infrastructure setup, test users, and RLS configuration.

```bash
# Start infrastructure services
cd docker && docker compose up -d

# Apply EF Core migrations
dotnet ef database update --project src/TadHub.Infrastructure --startup-project src/TadHub.Api

# Run API locally (with hot reload)
cd src/TadHub.Api && dotnet watch run
```

## Deployment

### Architecture

```
                        ┌──────────────┐
                        │   Traefik    │ ← SSL termination, routing
                        │  (reverse    │
                        │   proxy)     │
                        └──┬───┬───┬───┘
                           │   │   │
        ┌──────────────────┘   │   └──────────────────┐
        ▼                      ▼                      ▼
┌───────────────┐   ┌───────────────┐   ┌───────────────┐
│  Tenant App   │   │   .NET API    │   │    MinIO      │
│  (nginx/SPA)  │   │  (port 5000)  │   │  (storage)    │
│  port 80      │   │               │   │  port 9000    │
└───────────────┘   └───┬───┬───┬───┘   └───────────────┘
                        │   │   │
          ┌─────────────┘   │   └─────────────┐
          ▼                 ▼                 ▼
   ┌────────────┐   ┌────────────┐   ┌────────────┐
   │ PostgreSQL │   │   Redis    │   │  RabbitMQ  │
   │  (RLS)     │   │  (cache)   │   │  (events)  │
   └────────────┘   └────────────┘   └────────────┘
```

### Docker Compose Files

| File | Purpose |
|------|---------|
| `docker-compose.yml` | Infrastructure services (PostgreSQL, Redis, RabbitMQ, Keycloak, MinIO, Meilisearch, Grafana, Loki) |
| `docker-compose.override.yml` | Development overrides — adds API with hot reload + Mailpit for email testing |
| `docker-compose.prod.yml` | Production — adds API, Tenant App, MinIO, and Traefik with SSL |
| `docker-compose.coolify.yml` | Coolify PaaS deployment config |

### Dockerfiles

| File | Purpose | Base |
|------|---------|------|
| `Dockerfile.api` | .NET 9 API — multi-stage build (SDK build + Alpine runtime) | `mcr.microsoft.com/dotnet/aspnet:9.0-alpine` |
| `Dockerfile.tenant` | Tenant App — Node 22 build + nginx static serving | `nginx:alpine` |
| `Dockerfile.web` | Generic web app build (same pattern as tenant) | `nginx:alpine` |
| `Dockerfile.dev` | Development API with `dotnet watch` hot reload | `mcr.microsoft.com/dotnet/sdk:9.0` |

### Local Development

```bash
# 1. Start infrastructure only
cd docker
docker compose up -d postgres redis rabbitmq keycloak minio meilisearch

# 2. Apply migrations
cd ..
dotnet ef database update --project src/TadHub.Infrastructure --startup-project src/TadHub.Api

# 3. Run API with hot reload
cd src/TadHub.Api
dotnet watch run

# 4. Run frontend (separate terminal)
cd web/tenant-app
npm run dev
```

### Production Deployment (Coolify)

Production uses [Coolify](https://coolify.io/) as PaaS with its own Traefik reverse proxy (`coolify-proxy`). Containers must be on the `coolify` external network for the proxy to route traffic.

Routing is configured in a Traefik dynamic file (`/traefik/dynamic/tadhub.yaml` inside `coolify-proxy`), not via Docker labels.

```bash
cd docker

# 1. Configure environment
cp .env.example .env
# Edit .env with production credentials

# 2. Build containers (no cache for clean build)
docker compose -f docker-compose.coolify.yml build --no-cache api tenant-app

# 3. Apply database migrations
dotnet ef database update \
  --project src/TadHub.Infrastructure \
  --startup-project src/TadHub.Api \
  --connection "Host=localhost;Database=tadhub;Username=<user>;Password=<pass>"

# 4. Deploy
docker compose -f docker-compose.coolify.yml up -d

# 5. Verify health
docker compose -f docker-compose.coolify.yml ps
curl https://api.endlessmaker.com/health
curl https://tadbeer.endlessmaker.com/
```

> **Important:** Do NOT use `docker-compose.prod.yml` for production — it creates containers on
> `tadhub-network` which Coolify's proxy cannot reach. Always use `docker-compose.coolify.yml`
> which connects to the `coolify` external network.

### Deploying Code Changes

After modifying backend or frontend code:

```bash
cd docker

# Rebuild only the changed services
docker compose -f docker-compose.coolify.yml build --no-cache api tenant-app

# Apply any new migrations (if schema changed)
dotnet ef database update \
  --project src/TadHub.Infrastructure \
  --startup-project src/TadHub.Api \
  --connection "Host=localhost;Database=<db>;Username=<user>;Password=<pass>"

# Restart with new images
docker compose -f docker-compose.coolify.yml up -d api tenant-app
```

### Production URLs

| Service | URL |
|---------|-----|
| API | https://api.endlessmaker.com |
| Tenant App | https://tadbeer.endlessmaker.com |
| Auth (Keycloak) | https://auth.endlessmaker.com |
| Storage (MinIO) | https://storage.endlessmaker.com |

### Health Checks

All containers include health checks:
- **API**: `GET /health` (30s interval, 30s start period)
- **Tenant App**: wget to `http://localhost/` (30s interval)
- **PostgreSQL**: `pg_isready` (10s interval)
- **Redis**: `redis-cli ping` (10s interval)
- **RabbitMQ**: `rabbitmq-diagnostics ping` (30s interval)

## Development

See [tasks/](./tasks/) for implementation plan and [PROGRESS.md](./PROGRESS.md) for current status.

## Timeline

- **Phase 0**: Boilerplate Adaptation (1 week)
- **Phase 1-2**: Client + Worker (3 weeks parallel)
- **Phase 3**: Contract Engine (3 weeks)
- **Phase 4-5**: Financial + PRO (3 weeks parallel)
- **Phase 6-7**: Scheduling + WPS (2 weeks parallel)
- **Phase 8-9**: Notifications + Reporting (2 weeks parallel)
- **Phase 10**: Integration Tests (1 week)

**Total: ~15 weeks**

## License

Proprietary - All rights reserved
