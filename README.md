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

```bash
# Start infrastructure
cd docker && docker-compose up -d

# Run API
cd src/TadHub.Api && dotnet run
```

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
