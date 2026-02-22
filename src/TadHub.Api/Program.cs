using System.Text.Json;
using System.Text.Json.Serialization;
using Analytics.Core;
using ApiManagement.Core;
using Audit.Core;
using Authorization.Core;
using Content.Core;
using FeatureFlags.Core;
using Identity.Core;
using Notification.Core;
using Portal.Core;
using Subscription.Core;
using _Template.Core;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Scalar.AspNetCore;
using TadHub.Infrastructure;
using TadHub.Infrastructure.Settings;
using TadHub.Infrastructure.Tenancy;
using Tenancy.Core;
using Worker.Core;
using ClientManagement.Core;
using ReferenceData.Core;
using Supplier.Core;

var builder = WebApplication.CreateBuilder(args);

// =============================================================================
// Configuration - Bind strongly-typed settings
// =============================================================================
var keycloakSettings = builder.Configuration.GetSection(KeycloakSettings.SectionName).Get<KeycloakSettings>()!;
var corsSettings = builder.Configuration.GetSection(CorsSettings.SectionName).Get<CorsSettings>()!;
var featureSettings = builder.Configuration.GetSection(FeatureSettings.SectionName).Get<FeatureSettings>()!;

// =============================================================================
// Controllers with JSON options
// =============================================================================
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
        options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
    });

// =============================================================================
// Authentication - JWT Bearer with Keycloak
// =============================================================================
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.Authority = keycloakSettings.Authority;
        options.Audience = keycloakSettings.Audience;
        options.RequireHttpsMetadata = keycloakSettings.RequireHttpsMetadata;

        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = keycloakSettings.ValidateIssuer,
            ValidateAudience = keycloakSettings.ValidateAudience,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            // Use standard ClaimTypes.Role for role validation
            RoleClaimType = System.Security.Claims.ClaimTypes.Role
        };
        
        // Map Keycloak realm_access roles to standard role claims
        options.Events = new JwtBearerEvents
        {
            OnTokenValidated = context =>
            {
                var realmAccess = context.Principal?.FindFirst("realm_access")?.Value;
                if (!string.IsNullOrEmpty(realmAccess))
                {
                    try
                    {
                        var doc = System.Text.Json.JsonDocument.Parse(realmAccess);
                        if (doc.RootElement.TryGetProperty("roles", out var rolesArray))
                        {
                            var identity = context.Principal?.Identity as System.Security.Claims.ClaimsIdentity;
                            foreach (var role in rolesArray.EnumerateArray())
                            {
                                var roleName = role.GetString();
                                if (!string.IsNullOrEmpty(roleName))
                                {
                                    identity?.AddClaim(new System.Security.Claims.Claim(
                                        System.Security.Claims.ClaimTypes.Role, roleName));
                                }
                            }
                        }
                    }
                    catch { /* ignore parsing errors */ }
                }
                return Task.CompletedTask;
            }
        };
    });

builder.Services.AddAuthorization();

// =============================================================================
// CORS
// =============================================================================
var allowedOrigins = corsSettings.GetEffectiveOrigins();
if (allowedOrigins.Length == 0)
{
    allowedOrigins = ["http://localhost:3000"];
}

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.WithOrigins(allowedOrigins)
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });
});

// =============================================================================
// OpenAPI / Scalar
// =============================================================================
builder.Services.AddOpenApi();

// =============================================================================
// Infrastructure Services (EF Core, Redis, MassTransit, Hangfire, MinIO, etc.)
// =============================================================================
builder.Services.AddInfrastructure(builder.Configuration);

// =============================================================================
// Module Services
// =============================================================================
builder.Services.AddIdentityModule();
builder.Services.AddTenancyModule();
builder.Services.AddAuthorizationModule();
builder.Services.AddNotificationModule();
builder.Services.AddSubscriptionModule(builder.Configuration);
builder.Services.AddPortalModule();
builder.Services.AddApiManagementModule();
builder.Services.AddFeatureFlagsModule();
builder.Services.AddAuditModule();
builder.Services.AddAnalyticsModule();
builder.Services.AddContentModule();
builder.Services.AddTemplateModule();

// Supplier module
builder.Services.AddSupplierModule();

// Reference data module
builder.Services.AddReferenceDataModule();

// Tadbeer domain modules
builder.Services.AddWorkerModule();
builder.Services.AddClientManagementModule();

var app = builder.Build();

// =============================================================================
// Database Seeding (Reference Data)
// =============================================================================
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var logger = services.GetRequiredService<ILogger<Program>>();
    
    try
    {
        // Seed reference data (countries, job categories)
        var countrySeeder = services.GetRequiredService<ReferenceData.Core.Seeds.CountrySeeder>();
        var jobCategorySeeder = services.GetRequiredService<ReferenceData.Core.Seeds.JobCategorySeeder>();
        
        await jobCategorySeeder.SeedAsync();
        await countrySeeder.SeedAsync();
        
        logger.LogInformation("Reference data seeding completed");
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "An error occurred while seeding reference data");
        // Don't throw - allow app to start even if seeding fails
    }
}

// =============================================================================
// Middleware Pipeline
// =============================================================================

// Infrastructure middleware (exception handler, response wrapping)
app.UseInfrastructure(builder.Configuration);

app.UseCors();
app.UseAuthentication();
app.UseAuthorization();

// Tenant resolution (after auth, so JWT claims are available)
app.UseMiddleware<TenantResolutionMiddleware>();

// =============================================================================
// Basic Health Endpoint (quick liveness check)
// =============================================================================
app.MapGet("/health", () => Results.Ok(new
{
    status = "healthy",
    timestamp = DateTime.UtcNow,
    version = typeof(Program).Assembly.GetName().Version?.ToString() ?? "1.0.0"
}))
.WithTags("Health")
.AllowAnonymous();

// =============================================================================
// OpenAPI / Scalar (Development only)
// =============================================================================
if (app.Environment.IsDevelopment() && featureSettings.EnableSwagger)
{
    app.MapOpenApi();
    app.MapScalarApiReference(options =>
    {
        options.WithTitle("SaaS Boilerplate API")
            .WithTheme(ScalarTheme.BluePlanet)
            .WithDefaultHttpClient(ScalarTarget.CSharp, ScalarClient.HttpClient);
    });
}

// =============================================================================
// Controllers
// =============================================================================
app.MapControllers();

// =============================================================================
// Infrastructure Endpoints (detailed health checks, Hangfire dashboard)
// =============================================================================
app.MapInfrastructureEndpoints(builder.Configuration);

// =============================================================================
// Run
// =============================================================================
app.Run();

// Make Program class accessible for integration tests
public partial class Program { }
