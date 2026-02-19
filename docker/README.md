# Docker Development Environment

This folder contains the Docker Compose configuration for the SaaS Boilerplate development environment.

## Quick Start

```bash
# Copy environment file
cp .env.example .env

# Start all services
docker compose up -d

# View logs
docker compose logs -f

# Stop all services
docker compose down
```

## Services

| Service | Port | URL | Credentials |
|---------|------|-----|-------------|
| **PostgreSQL** | 5432 | - | saaskit / saaskit_dev |
| **Keycloak** | 8080 | http://localhost:8080 | admin / admin |
| **Redis** | 6379 | - | password: redis_dev |
| **RabbitMQ** | 5672, 15672 | http://localhost:15672 | saaskit / rabbitmq_dev |
| **MinIO** | 9000, 9001 | http://localhost:9001 | minioadmin / minioadmin |
| **Meilisearch** | 7700 | http://localhost:7700 | API Key: meilisearch_dev_key |
| **Grafana** | 3001 | http://localhost:3001 | admin / admin |
| **Loki** | 3100 | - | - |
| **Mailpit** | 8025, 1025 | http://localhost:8025 | - |

## Running the API

### Option 1: With Docker (includes hot reload)

```bash
docker compose -f docker-compose.yml -f docker-compose.override.yml up -d
```

### Option 2: Local Development (recommended)

```bash
# Start infrastructure only
docker compose up -d postgres redis rabbitmq keycloak minio meilisearch

# Run API locally
cd ../src/TadHub.Api
dotnet watch run
```

## Keycloak Test Users

| Email | Password | Role |
|-------|----------|------|
| admin@saaskit.dev | Admin123! | platform-admin |
| user@saaskit.dev | User123! | platform-user |

## Database Setup

### After First Run

1. **Run EF Migrations** (creates tables):
   ```bash
   cd ../src/TadHub.Api
   dotnet ef database update
   ```

2. **Apply RLS Policies** (enables row-level security):
   ```bash
   docker compose exec postgres psql -U saaskit -d saaskit_dev -f /docker-entrypoint-initdb.d/rls-policies.sql
   ```

   Or from host:
   ```bash
   psql -h localhost -U saaskit -d saaskit_dev -f postgres/rls-policies.sql
   ```

### Row Level Security (RLS)

RLS provides defense-in-depth multi-tenancy at the database level. Even raw SQL queries cannot cross tenant boundaries.

**How it works:**
- EF Core's `RlsInterceptor` sets `app.current_tenant_id` on each connection
- PostgreSQL RLS policies filter rows based on this session variable
- Without a tenant context, queries return zero rows

**Verify RLS is active:**
```sql
SELECT schemaname, tablename, rowsecurity 
FROM pg_tables 
WHERE schemaname = 'public' AND rowsecurity = true;
```

**Test RLS:**
```sql
-- Without tenant context (returns 0 rows)
SELECT * FROM tenant_users;

-- With tenant context (returns only that tenant's rows)
SET app.current_tenant_id = 'your-tenant-uuid';
SELECT * FROM tenant_users;

-- As platform admin (bypasses RLS)
SET app.is_platform_admin = 'true';
SELECT * FROM tenant_users;
```

## Useful Commands

```bash
# View service status
docker compose ps

# View logs for specific service
docker compose logs -f postgres

# Restart a specific service
docker compose restart keycloak

# Remove all volumes (clean start)
docker compose down -v

# Access PostgreSQL CLI
docker compose exec postgres psql -U saaskit -d saaskit_dev

# Access Redis CLI
docker compose exec redis redis-cli -a redis_dev
```

## Troubleshooting

### Keycloak won't start
- Ensure PostgreSQL is healthy first: `docker compose ps`
- Check Keycloak logs: `docker compose logs keycloak`

### Port conflicts
- Edit `.env` file to change ports if needed
- Check for existing services: `netstat -tulpn | grep LISTEN`

### Reset everything
```bash
docker compose down -v
docker compose up -d
```
