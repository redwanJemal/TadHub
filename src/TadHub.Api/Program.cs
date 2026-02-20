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
            ValidateIssuerSigningKey = true
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

// Tadbeer domain modules
builder.Services.AddWorkerModule();
builder.Services.AddClientManagementModule();

var app = builder.Build();

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
