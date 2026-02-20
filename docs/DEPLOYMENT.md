# TadHub Deployment Guide

## Production URLs

| Service | URL | Status |
|---------|-----|--------|
| Keycloak (Auth) | https://auth.endlessmaker.com | ✅ Live |
| Tenant App | https://tadbeer.endlessmaker.com | ✅ Placeholder |
| API | https://api.endlessmaker.com | ✅ Placeholder |
| Backoffice | https://admin.endlessmaker.com | ✅ Placeholder |
| Storage (MinIO) | https://storage.endlessmaker.com | ✅ Shared |

## Coolify Service

- **Project:** TadHub
- **Service UUID:** `vc4kw0wggk0ccgow0sw080cg`
- **Service Name:** tadhub-stack

## Container Names

| Container | Image | Purpose |
|-----------|-------|---------|
| postgres-vc4kw0wggk0ccgow0sw080cg | postgres:17-alpine | Database |
| redis-vc4kw0wggk0ccgow0sw080cg | redis:7-alpine | Cache |
| rabbitmq-vc4kw0wggk0ccgow0sw080cg | rabbitmq:3-management-alpine | Message Broker |
| keycloak-vc4kw0wggk0ccgow0sw080cg | quay.io/keycloak/keycloak:26.0 | Identity Provider |
| api-vc4kw0wggk0ccgow0sw080cg | nginx:alpine (placeholder) | Backend API |
| tenant-app-vc4kw0wggk0ccgow0sw080cg | nginx:alpine (placeholder) | Tenant Frontend |
| backoffice-app-vc4kw0wggk0ccgow0sw080cg | nginx:alpine (placeholder) | Admin Frontend |

## Credentials

**Location:** `/root/projects/TadHub/.env.production`

### Keycloak Admin
- **URL:** https://auth.endlessmaker.com/admin/
- **Username:** admin
- **Password:** See `.env.production`

### Database (PostgreSQL)
- **Host:** postgres-vc4kw0wggk0ccgow0sw080cg
- **Database:** tadhub
- **User:** tadhub
- **Password:** See `.env.production`

### Redis
- **Host:** redis-vc4kw0wggk0ccgow0sw080cg
- **Password:** See `.env.production`

### RabbitMQ
- **Host:** rabbitmq-vc4kw0wggk0ccgow0sw080cg
- **User:** tadhub
- **Password:** See `.env.production`
- **Management UI:** Internal only (port 15672)

### MinIO (Shared Storage)
- **Endpoint:** https://storage.endlessmaker.com
- **Credentials:** See `.env.production`

## Traefik Configuration

Traefik routes are managed in `/traefik/dynamic/tadhub.yaml` on the coolify-proxy container.

**Important:** Container IPs can change on restart. After restarting containers, update the Traefik config:

```bash
# Get new IPs
KC_IP=$(docker inspect keycloak-vc4kw0wggk0ccgow0sw080cg -f '{{.NetworkSettings.Networks.coolify.IPAddress}}')
API_IP=$(docker inspect api-vc4kw0wggk0ccgow0sw080cg -f '{{.NetworkSettings.Networks.coolify.IPAddress}}')
TENANT_IP=$(docker inspect tenant-app-vc4kw0wggk0ccgow0sw080cg -f '{{.NetworkSettings.Networks.coolify.IPAddress}}')
BACKOFFICE_IP=$(docker inspect backoffice-app-vc4kw0wggk0ccgow0sw080cg -f '{{.NetworkSettings.Networks.coolify.IPAddress}}')

# Update Traefik (create new config file and copy)
docker cp /tmp/tadhub-routes.yaml coolify-proxy:/traefik/dynamic/tadhub.yaml
```

## Deployment Commands

### Redeploy via Coolify API
```bash
source /root/clawd/skills/coolify/.env
curl -s -X POST -H "Authorization: Bearer $COOLIFY_API_KEY" \
  "http://localhost:8000/api/v1/deploy?uuid=vc4kw0wggk0ccgow0sw080cg&force=true"
```

### Manual Docker Compose
```bash
cd /data/coolify/services/vc4kw0wggk0ccgow0sw080cg
docker compose up -d
docker compose ps
```

### View Logs
```bash
# All services
docker compose logs -f

# Specific service
docker logs -f keycloak-vc4kw0wggk0ccgow0sw080cg
```

## Next Steps

### 1. Configure Keycloak Realm
1. Go to https://auth.endlessmaker.com/admin/
2. Login with admin credentials
3. Create realm "tadhub"
4. Create clients:
   - `tenant-app` (public client, SPA)
   - `backoffice-app` (public client, SPA)
   - `tadhub-api` (confidential client)

### 2. Deploy Real Applications
Replace nginx placeholders with actual apps:

1. Build and push Docker images:
   ```bash
   # API
   docker build -t ghcr.io/redwanjemal/tadhub-api:latest -f docker/Dockerfile.api .
   docker push ghcr.io/redwanjemal/tadhub-api:latest

   # Tenant App
   cd web/tenant-app
   docker build -t ghcr.io/redwanjemal/tadhub-tenant:latest -f ../../docker/Dockerfile.web .
   docker push ghcr.io/redwanjemal/tadhub-tenant:latest
   ```

2. Update docker-compose.yml with real images
3. Redeploy

### 3. Database Migrations
Run .NET migrations after API is deployed:
```bash
docker exec api-vc4kw0wggk0ccgow0sw080cg dotnet TadHub.Api.dll --migrate
```

## Troubleshooting

### 502 Bad Gateway
- Check if containers are running: `docker ps | grep vc4kw0wggk0ccgow0sw080cg`
- Verify Traefik IPs are correct
- Check for conflicting Traefik configs

### Keycloak Not Starting
- Check postgres is healthy first
- View logs: `docker logs keycloak-vc4kw0wggk0ccgow0sw080cg`
- Ensure KC_DB_PASSWORD matches POSTGRES_PASSWORD

### Container Network Issues
- All TadHub containers must be on `coolify` network for Traefik routing
- Internal services also need `tadhub-internal` network

## File Locations

| File | Purpose |
|------|---------|
| `/root/projects/TadHub/.env.production` | Production credentials |
| `/data/coolify/services/vc4kw0wggk0ccgow0sw080cg/` | Coolify service directory |
| `/data/coolify/services/vc4kw0wggk0ccgow0sw080cg/.env` | Service environment file |
| `/data/coolify/services/vc4kw0wggk0ccgow0sw080cg/docker-compose.yml` | Active docker-compose |
