# TadHub Backend

**Production-ready, multi-tenant SaaS backend with .NET 9**

[![.NET](https://img.shields.io/badge/.NET-9.0-512BD4)](https://dotnet.microsoft.com/)
[![License](https://img.shields.io/badge/License-MIT-green.svg)](LICENSE)

---

## 📊 Project Status

| Metric | Value |
|--------|-------|
| **Tasks** | 55 |
| **Phases** | 15 |
| **Entities** | ~71 |
| **Progress** | See [PROGRESS.md](PROGRESS.md) |

## 🏗️ Tech Stack

| Layer | Technology |
|-------|------------|
| **Runtime** | .NET 9 |
| **Database** | PostgreSQL 17 |
| **Identity** | Keycloak 26 |
| **Cache** | Redis 7 |
| **Messaging** | RabbitMQ + MassTransit |
| **Storage** | MinIO (S3-compatible) |
| **Search** | Meilisearch |
| **Background Jobs** | Hangfire |
| **Observability** | Grafana + Loki |

## 🏛️ Architecture

```
┌─────────────────────────────────────────────────────────────┐
│                         API Layer                           │
│   Controllers → Thin, validation + service call + return    │
├─────────────────────────────────────────────────────────────┤
│                      Module Layer                           │
│   Identity │ Tenancy │ Authorization │ Subscription │ ...   │
│   Each module: Contracts (public) + Core (implementation)   │
├─────────────────────────────────────────────────────────────┤
│                    Infrastructure Layer                     │
│   EF Core │ MassTransit │ Redis │ MinIO │ Meilisearch      │
├─────────────────────────────────────────────────────────────┤
│                     SharedKernel Layer                      │
│   Base entities │ Interfaces │ Domain events │ Extensions   │
└─────────────────────────────────────────────────────────────┘
```

## 🚀 Quick Start

### Prerequisites

- [.NET 9 SDK](https://dotnet.microsoft.com/download/dotnet/9.0)
- [Docker](https://www.docker.com/get-started) & Docker Compose
- Git

### 1. Clone the Repository

```bash
git clone https://github.com/redwanJemal/TadHub.git
cd TadHub
```

### 2. Start Infrastructure Services

```bash
cd docker
cp .env.example .env
docker compose up -d
```

### 3. Run the API

```bash
cd ../src/SaasKit.Api
dotnet run
```

### 4. Access the Application

| Service | URL | Credentials |
|---------|-----|-------------|
| **API** | http://localhost:5000 | - |
| **API Docs** | http://localhost:5000/scalar/v1 | - |
| **Health Check** | http://localhost:5000/health | - |
| **Keycloak** | http://localhost:8080 | admin / admin |
| **RabbitMQ** | http://localhost:15672 | saaskit / rabbitmq_dev |
| **MinIO** | http://localhost:9001 | minioadmin / minioadmin |
| **Grafana** | http://localhost:3001 | admin / admin |

### Test Users

| Email | Password | Role |
|-------|----------|------|
| admin@saaskit.dev | Admin123! | platform-admin |
| user@saaskit.dev | User123! | platform-user |

## 📁 Project Structure

```
TadHub/
├── src/
│   ├── SaasKit.Api/                 # ASP.NET Core Web API
│   ├── SaasKit.SharedKernel/        # Shared types & interfaces
│   ├── SaasKit.Infrastructure/      # Cross-cutting concerns
│   └── Modules/
│       ├── Identity/                # User management
│       ├── Tenancy/                 # Multi-tenancy
│       ├── Authorization/           # RBAC + Permissions
│       ├── Notification/            # SSE + Email
│       ├── Subscription/            # Plans + Stripe
│       ├── Portal/                  # B2B2C portals
│       ├── ApiManagement/           # API keys + Rate limiting
│       ├── FeatureFlags/            # Feature toggles
│       ├── Audit/                   # Event logging + Webhooks
│       ├── Analytics/               # Usage tracking
│       ├── Content/                 # Blog + KB + Pages
│       └── _Template/               # Module template
├── tests/
│   ├── SaasKit.Tests.Unit/
│   └── SaasKit.Tests.Integration/
├── docker/
│   ├── docker-compose.yml
│   ├── docker-compose.override.yml
│   ├── postgres/init.sql
│   └── keycloak/realm-export.json
└── tasks/                           # Implementation task cards
```

## 🎯 Modules

| Module | Description | Entities |
|--------|-------------|----------|
| **Identity** | User profiles, Keycloak sync | 2 |
| **Tenancy** | Multi-tenant organizations | 6 |
| **Authorization** | RBAC, permissions, groups | 7 |
| **Notification** | SSE push, email templates | 1 |
| **Subscription** | Plans, Stripe, feature gates | 10 |
| **Portal** | B2B2C white-label portals | 10 |
| **ApiManagement** | API keys, rate limiting | 2 |
| **FeatureFlags** | Progressive rollouts | 2 |
| **Audit** | Event logging, webhooks | 5 |
| **Analytics** | Page views, events | 4 |
| **Content** | Blog, KB, pages | 16 |

## 📋 API Conventions

### Response Envelope

```json
{
  "data": { ... },
  "meta": {
    "timestamp": "2026-02-19T10:30:00Z",
    "requestId": "req_f7e2c..."
  }
}
```

### Paginated Response

```json
{
  "data": [ ... ],
  "meta": { ... },
  "pagination": {
    "page": 1,
    "pageSize": 20,
    "totalCount": 142,
    "totalPages": 8,
    "hasNextPage": true,
    "hasPreviousPage": false
  }
}
```

### Error Response (RFC 9457)

```json
{
  "type": "https://api.saaskit.dev/errors/validation",
  "title": "Validation Failed",
  "status": 422,
  "detail": "One or more fields have invalid values.",
  "errors": {
    "name": ["Name is required."]
  }
}
```

### Filtering & Sorting

```http
GET /api/v1/tenants?filter[status]=active&filter[status]=suspended&sort=-createdAt&page=2&pageSize=50
```

## 🧪 Testing

```bash
# Run all tests
dotnet test

# Run with coverage
dotnet test --collect:"XPlat Code Coverage"

# Run specific category
dotnet test --filter "Category=Unit"
```

## 🔧 Development

### Build

```bash
dotnet build
```

### Run Migrations

```bash
cd src/SaasKit.Api
dotnet ef database update
```

### Format Code

```bash
dotnet format
```

## 📚 Documentation

- [Task Cards](./tasks/) - Implementation details
- [Progress Tracker](./PROGRESS.md) - Current status
- [Docker Guide](./docker/README.md) - Infrastructure setup

## 🤝 Contributing

1. Fork the repository
2. Create a feature branch
3. Commit your changes
4. Push to the branch
5. Open a Pull Request

## 📄 License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

---

Built with ❤️ using .NET 9
