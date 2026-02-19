# Phase 0: Solution Scaffolding + Docker

Create the .NET solution structure, all project files with correct references, and the Docker Compose development environment.

**Goal:** After this phase, `dotnet build` compiles cleanly and `docker compose up` starts all infrastructure services.

---

## P0-T01: Create solution and all project files

### Dependencies
None

### Files to Create
```
saas-boilerplate.sln
src/SaasKit.Api/SaasKit.Api.csproj
src/SaasKit.SharedKernel/SaasKit.SharedKernel.csproj
src/SaasKit.Infrastructure/SaasKit.Infrastructure.csproj
src/Modules/**/Contracts + Core .csproj (24 module projects)
tests/SaasKit.Tests.Unit/SaasKit.Tests.Unit.csproj
tests/SaasKit.Tests.Integration/SaasKit.Tests.Integration.csproj
```

### Instructions

1. Create solution:
   ```bash
   dotnet new sln -n saas-boilerplate
   ```

2. Create SharedKernel:
   ```bash
   dotnet new classlib -n SaasKit.SharedKernel -o src/SaasKit.SharedKernel
   ```

3. Create Infrastructure:
   ```bash
   dotnet new classlib -n SaasKit.Infrastructure -o src/SaasKit.Infrastructure
   ```

4. Create API:
   ```bash
   dotnet new webapi -n SaasKit.Api -o src/SaasKit.Api
   ```

5. Create module projects (for each module: Identity, Tenancy, Authorization, Notification, Subscription, Portal, ApiManagement, FeatureFlags, Audit, Analytics, Content, _Template):
   ```bash
   # Example for Identity
   dotnet new classlib -n Identity.Contracts -o src/Modules/Identity/Identity.Contracts
   dotnet new classlib -n Identity.Core -o src/Modules/Identity/Identity.Core
   ```

6. Create test projects:
   ```bash
   dotnet new xunit -n SaasKit.Tests.Unit -o tests/SaasKit.Tests.Unit
   dotnet new xunit -n SaasKit.Tests.Integration -o tests/SaasKit.Tests.Integration
   ```

7. Set up project references:
   - **Contracts projects**: Reference only SharedKernel. Zero NuGet packages.
   - **Core projects**: Reference own Contracts + SharedKernel + Infrastructure
   - **Api**: Reference Infrastructure + all Core projects
   - **Tests**: Reference Api, Infrastructure, Core projects

8. Add all projects to solution:
   ```bash
   dotnet sln add **/*.csproj
   ```

### Project Reference Matrix

| Project | References |
|---------|------------|
| *.Contracts | SharedKernel only |
| *.Core | Own Contracts, SharedKernel, Infrastructure, other modules' Contracts (never Core) |
| Infrastructure | SharedKernel |
| Api | Infrastructure, all Core projects |

### Tests
- [ ] `dotnet build` compiles with zero errors
- [ ] All 28+ projects are in the solution

### Acceptance Criteria
`dotnet build` from solution root succeeds with zero errors and zero warnings about missing references.

---

## P0-T02: Create Directory.Build.props and global configuration

### Dependencies
P0-T01

### Files to Create
```
Directory.Build.props
Directory.Packages.props
.editorconfig
global.json
```

### Instructions

1. **Directory.Build.props** - Apply to all projects:
   ```xml
   <Project>
     <PropertyGroup>
       <TargetFramework>net9.0</TargetFramework>
       <Nullable>enable</Nullable>
       <ImplicitUsings>enable</ImplicitUsings>
       <TreatWarningsAsErrors>true</TreatWarningsAsErrors>
     </PropertyGroup>
   </Project>
   ```

2. **Directory.Packages.props** - Central package management:
   ```xml
   <Project>
     <PropertyGroup>
       <ManagePackageVersionsCentrally>true</ManagePackageVersionsCentrally>
     </PropertyGroup>
     <ItemGroup>
       <PackageVersion Include="Microsoft.EntityFrameworkCore" Version="9.0.*" />
       <PackageVersion Include="Npgsql.EntityFrameworkCore.PostgreSQL" Version="9.0.*" />
       <PackageVersion Include="MassTransit" Version="8.3.*" />
       <PackageVersion Include="MassTransit.RabbitMQ" Version="8.3.*" />
       <PackageVersion Include="StackExchange.Redis" Version="2.8.*" />
       <PackageVersion Include="FluentValidation.DependencyInjectionExtensions" Version="11.*" />
       <PackageVersion Include="Stripe.net" Version="46.*" />
       <PackageVersion Include="Minio" Version="6.*" />
       <PackageVersion Include="Meilisearch" Version="0.15.*" />
       <PackageVersion Include="Hangfire.AspNetCore" Version="1.8.*" />
       <PackageVersion Include="Hangfire.PostgreSql" Version="1.20.*" />
       <PackageVersion Include="Scalar.AspNetCore" Version="2.*" />
       <PackageVersion Include="Serilog.AspNetCore" Version="9.*" />
       <PackageVersion Include="xunit" Version="2.9.*" />
       <PackageVersion Include="FluentAssertions" Version="7.*" />
       <PackageVersion Include="NSubstitute" Version="5.*" />
       <PackageVersion Include="Testcontainers.PostgreSql" Version="4.*" />
       <PackageVersion Include="Microsoft.AspNetCore.Mvc.Testing" Version="9.0.*" />
       <PackageVersion Include="Bogus" Version="35.*" />
     </ItemGroup>
   </Project>
   ```

3. **global.json** - Pin SDK:
   ```json
   {
     "sdk": {
       "version": "9.0.100",
       "rollForward": "latestMinor"
     }
   }
   ```

4. **.editorconfig** - C# coding style (file-scoped namespaces, var preferences)

### Tests
- [ ] Build compiles
- [ ] Package versions centrally managed

### Acceptance Criteria
`dotnet build` succeeds. No project has a hardcoded package version.

---

## P0-T03: Create Docker Compose for development environment

### Dependencies
P0-T01

### Files to Create
```
docker/docker-compose.yml
docker/docker-compose.override.yml
docker/.env
docker/postgres/init.sql
docker/keycloak/realm-export.json
```

### Instructions

1. **docker-compose.yml** - 8 services:
   - PostgreSQL 17 (port 5432)
   - Keycloak 26 (port 8080)
   - Redis/Valkey (port 6379)
   - RabbitMQ 3-management (ports 5672, 15672)
   - MinIO (ports 9000, 9001)
   - Meilisearch (port 7700)
   - Grafana (port 3001)
   - Loki

2. **PostgreSQL configuration**:
   ```yaml
   postgres:
     image: postgres:17
     environment:
       POSTGRES_USER: saaskit
       POSTGRES_PASSWORD: saaskit_dev
       POSTGRES_DB: saaskit_dev
     ports:
       - "5432:5432"
     volumes:
       - ./postgres/init.sql:/docker-entrypoint-initdb.d/init.sql
   ```

3. **init.sql**:
   ```sql
   CREATE EXTENSION IF NOT EXISTS "uuid-ossp";
   CREATE EXTENSION IF NOT EXISTS "pgcrypto";
   ```

4. **docker-compose.override.yml** - .NET API service with hot reload:
   ```yaml
   api:
     build: ../src/SaasKit.Api
     volumes:
       - ../src:/src
     environment:
       - DOTNET_WATCH_RESTART_ON_RUDE_EDIT=true
   ```

5. **.env** - All connection strings and passwords

### Tests
- [ ] `docker compose up -d` - all 8 services healthy

### Acceptance Criteria
All containers healthy. Keycloak at :8080, RabbitMQ mgmt at :15672, MinIO at :9001.

---

## P0-T04: Create Keycloak realm export with full client configuration

### Dependencies
P0-T03

### Files to Create
```
docker/keycloak/realm-export.json
```

### Instructions

Create realm export with:

1. **Realm**: `saas-platform`

2. **Clients**:
   | Client ID | Type | Flow |
   |-----------|------|------|
   | saas-api | Confidential | Client Credentials |
   | saas-web | Public | PKCE |
   | saas-portal | Public | PKCE |
   | saas-admin | Confidential | MFA Required |

3. **Realm Roles**:
   - `platform-admin`
   - `platform-user`

4. **Client Scope**: `tenant_id` as optional claim mapper on `saas-web`

5. **Test Users**:
   | Email | Password | Role |
   |-------|----------|------|
   | admin@saaskit.dev | Admin123! | platform-admin |
   | user@saaskit.dev | User123! | platform-user |

6. **Token Lifetimes**:
   - Access: 5 minutes
   - Refresh: 30 minutes
   - SSO: 8 hours

### Tests
- [ ] Test users authenticate via Keycloak token endpoint
- [ ] JWT contains realm_access.roles

### Acceptance Criteria
POST to token endpoint returns JWT with realm_access.roles.

---

## P0-T05: Create appsettings.json with all config sections

### Dependencies
P0-T03

### Files to Create
```
src/SaasKit.Api/appsettings.json
src/SaasKit.Api/appsettings.Development.json
```

### Instructions

1. **Config sections**:
   - ConnectionStrings (Postgres, Redis)
   - Keycloak (Authority, Audience, ClientId, ClientSecret)
   - RabbitMq (Host, Username, Password)
   - Minio (Endpoint, AccessKey, SecretKey)
   - Meilisearch (Url, ApiKey)
   - Hangfire (ConnectionString)
   - Logging

2. **Development overrides** with localhost values matching docker-compose

3. Each section maps to a strongly-typed settings class in Infrastructure:
   - `KeycloakSettings`
   - `RabbitMqSettings`
   - `MinioSettings`
   - `MeilisearchSettings`

### Tests
- [ ] App starts and reads all config values

### Acceptance Criteria
Program.cs resolves each settings class from DI without null values.

---

## P0-T06: Create minimal Program.cs that boots cleanly

### Dependencies
P0-T01, P0-T05

### Files to Create
```
src/SaasKit.Api/Program.cs
```

### Instructions

1. **Minimal Program.cs**:
   ```csharp
   var builder = WebApplication.CreateBuilder(args);
   
   // Controllers with JSON options
   builder.Services.AddControllers()
       .AddJsonOptions(options =>
       {
           options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
           options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
           options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
       });
   
   // JWT Bearer auth pointing to Keycloak
   builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
       .AddJwtBearer(options =>
       {
           options.Authority = builder.Configuration["Keycloak:Authority"];
           options.Audience = builder.Configuration["Keycloak:Audience"];
       });
   
   builder.Services.AddAuthorization();
   
   // CORS for localhost:3000
   builder.Services.AddCors(options =>
   {
       options.AddDefaultPolicy(policy =>
       {
           policy.WithOrigins("http://localhost:3000")
               .AllowAnyHeader()
               .AllowAnyMethod();
       });
   });
   
   var app = builder.Build();
   
   app.UseCors();
   app.UseAuthentication();
   app.UseAuthorization();
   app.MapControllers();
   
   // Health endpoint
   app.MapGet("/health", () => Results.Ok(new { status = "healthy" }));
   
   // Scalar API docs
   app.MapScalarApiReference();
   
   app.Run();
   ```

2. Do NOT register modules yet - just get a clean boot

### Tests
- [ ] GET /health returns 200
- [ ] Scalar loads at /api-docs

### Acceptance Criteria
`curl localhost:5000/health` returns 200.

---

## P0-T07: Create .gitignore and README

### Dependencies
P0-T01

### Files to Create
```
.gitignore
README.md (already created at root level)
```

### Instructions

1. **.gitignore** - .NET + Node ignores:
   ```gitignore
   # .NET
   bin/
   obj/
   *.user
   *.suo
   .vs/
   
   # IDE
   .idea/
   *.swp
   
   # Build
   publish/
   
   # Secrets
   appsettings.*.local.json
   .env.local
   
   # Test results
   TestResults/
   coverage/
   
   # Docker
   docker/.env
   
   # OS
   .DS_Store
   Thumbs.db
   ```

2. **README** - Already created at project root

### Tests
N/A

### Acceptance Criteria
README accurately describes how to run the project.

---

## Phase 0 Checklist

- [ ] P0-T01: Solution and project files created
- [ ] P0-T02: Global configuration set up
- [ ] P0-T03: Docker Compose working
- [ ] P0-T04: Keycloak realm configured
- [ ] P0-T05: App settings configured
- [ ] P0-T06: API boots cleanly
- [ ] P0-T07: .gitignore and README created
- [ ] `dotnet build` succeeds
- [ ] `docker compose up -d` all services healthy
- [ ] `/health` returns 200
