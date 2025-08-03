using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.HttpLogging;
using Microsoft.AspNetCore.Identity;
using NiyaCRM.Core.Identity;
using Microsoft.IdentityModel.Tokens;
using NiyaCRM.Api.Configurations;
using NiyaCRM.Infrastructure.Logging.Serilog;
using NiyaCRM.Infrastructure.Data;
using NiyaCRM.Core.Tenants;
using NiyaCRM.Application.Tenants;
using FluentValidation;
using NiyaCRM.Application.Tenants.Validators;
using NiyaCRM.Api.Validators.Tenants;
using NiyaCRM.Infrastructure.Data.Tenants;
using NiyaCRM.Core.AuditLogs;
using NiyaCRM.Application.AuditLogs;
using NiyaCRM.Infrastructure.Data.AuditLogs;
using NiyaCRM.Core.DynamicObjects;
using NiyaCRM.Application.DynamicObjects;
using NiyaCRM.Infrastructure.Data.DynamicObjects;
using NiyaCRM.Application.Identity;
using NiyaCRM.Infrastructure.Data.Identity;
using NiyaCRM.Core;
using Serilog;
using System.Reflection;
using System.Text;
using NiyaCRM.Core.Tenants.DTOs;
using NiyaCRM.Api.Middleware;
using NiyaCRM.Api.Helpers;
using NiyaCRM.Core.Auth.Constants;
using NiyaCRM.Core.Common;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using NiyaCRM.Api.Conventions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.OpenApi.Models;
using NiyaCRM.AppInstallation;
using Swashbuckle.AspNetCore.SwaggerGen;

var builder = WebApplication.CreateBuilder(args);

builder.AddConfigurations();

Log.Error("Server Booting Up...");

// HTTP Loging Config
builder.Services.AddHttpLogging(logging =>
{
    logging.LoggingFields = HttpLoggingFields.All;
    logging.RequestHeaders.Add("x-correlation-id");
    logging.RequestHeaders.Add("X-Forwarded-For");
    logging.RequestHeaders.Add("X-Forwarded-Proto");
    logging.RequestHeaders.Add("X-Forwarded-Port");
    logging.RequestHeaders.Add("X-Forwarded-Host");
    logging.RequestHeaders.Add("X-Forwarded-Server");
    logging.RequestHeaders.Add("X-Amzn-Trace-Id");
    logging.RequestHeaders.Add("Upgrade-Insecure-Requests");
    logging.RequestHeaders.Add("sec-ch-ua");
    logging.RequestHeaders.Add("sec-ch-ua-mobile");
    logging.RequestHeaders.Add("sec-ch-ua-platform");
    logging.ResponseHeaders.Add("x-correlation-id");
    logging.ResponseHeaders.Add("Pragma");
    logging.ResponseHeaders.Add("Cache-Control");
    logging.ResponseHeaders.Add("max-age");
});

builder.Services.AddControllersWithViews(options =>
{
    // Add a route convention to prefix all controller routes with 'api'
    // except for controllers that should be served directly
    options.Conventions.Add(new ApiControllerRouteConvention());
});

// Register validator for ActivateDeactivateTenantRequest
builder.Services.AddScoped<IValidator<ActivateDeactivateTenantRequest>, ActivateDeactivateTenantRequestValidator>();


// For http request context accessing
builder.Services.AddHttpContextAccessor();

// Register ApplicationDbContext with PostgreSQL
builder.Services.AddPostgreSqlDbContext(builder.Configuration);

// Configure Identity
builder.Services.AddIdentity<ApplicationUser, ApplicationRole>(options => {
    // Password settings
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireUppercase = true;
    options.Password.RequireNonAlphanumeric = true;
    options.Password.RequiredLength = 8;
    
    // Lockout settings
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
    options.Lockout.MaxFailedAccessAttempts = 5;
    
    // User settings
    options.User.RequireUniqueEmail = true;
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();

// Configure JWT Authentication
builder.Services.AddAuthentication(options => {
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options => {
    options.SaveToken = true;
    options.RequireHttpsMetadata = false;
    options.TokenValidationParameters = new TokenValidationParameters {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = AuthConstants.Jwt.Issuer,
        ValidAudience = AuthConstants.Jwt.Audience,
        IssuerSigningKey = new SymmetricSecurityKey(JwtHelper.GetJwtSigningKey())
    };
});

builder.Services.AddScoped<NiyaCRM.Core.Identity.IPermissionRepository, NiyaCRM.Infrastructure.Repositories.PermissionRepository>();
builder.Services.AddScoped<NiyaCRM.Core.Identity.IPermissionService, NiyaCRM.Application.Identity.PermissionService>();

// Add Authorization policies with global fallback policy using AuthorizationBuilder
var authBuilder = builder.Services.AddAuthorizationBuilder();

// This makes all endpoints require authentication by default
authBuilder.SetFallbackPolicy(new AuthorizationPolicyBuilder()
    .RequireAuthenticatedUser()
    .Build());

// Register Tenant Services
builder.Services.AddScoped<ITenantService, TenantService>();
builder.Services.AddScoped<ITenantRepository, TenantRepository>();

// Register AuditLog Services
builder.Services.AddScoped<IAuditLogService, AuditLogService>();
builder.Services.AddScoped<IAuditLogRepository, AuditLogRepository>();

// Register DynamicObject Services
builder.Services.AddScoped<IDynamicObjectService, DynamicObjectService>();
builder.Services.AddScoped<IDynamicObjectRepository, DynamicObjectRepository>();

// Register User Services
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IUserService, UserService>();

// Register Cache Services
builder.Services.AddMemoryCache();

// Register FluentValidation
builder.Services.AddScoped<IValidator<CreateTenantRequest>, CreateTenantRequestValidator>();
builder.Services.AddScoped<NiyaCRM.Core.Cache.ICacheRepository, NiyaCRM.Infrastructure.Cache.CacheRepository>();
builder.Services.AddScoped<NiyaCRM.Core.Cache.ICacheService, NiyaCRM.Application.Cache.CacheService>();

// Register Unit of Work
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

// Register JwtHelper
builder.Services.AddScoped<JwtHelper>();

// Register AppInstallation Services
builder.Services.AddAppInstallation();

// Register Swagger/OpenAPI
builder.Services.AddSwaggerServices(builder.Configuration);

// Register Health Checks
builder.Services.AddHealthChecks()
    .AddCheck("self", () => HealthCheckResult.Healthy(), tags: CommonConstant.HealthCheck.ServiceTags)
    .AddCheck<DatabaseHealthCheck>("database_connection", tags: CommonConstant.HealthCheck.DatabaseTags);

// Register the database health check as a service
builder.Services.AddScoped<DatabaseHealthCheck>();

// Register Serilog using the extension method
builder.Host.RegisterSerilog();

var app = builder.Build();

// Configure Swagger and Swagger UI
app.UseSwaggerMiddleware(app.Environment);

// Add CorrelationId to response headers for all requests
app.UseMiddleware<CorrelationIdMiddleware>();

// Add Domain to log context for all requests
app.UseMiddleware<DomainMiddleware>();

// Log all requests and response.
app.UseHttpLogging();

// Add JWT cookie authentication middleware
app.UseJwtCookieAuthentication();

// Enable authentication & authorization
app.UseAuthentication();
app.UseAuthorization();

// Streamlines framework logs into a single message per request, including path, method, timings, status code, and exception.
app.UseSerilogRequestLogging();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

// Initialise application (seed roles, etc.)
using (var scope = app.Services.CreateScope())
{
    var initialiser = scope.ServiceProvider.GetRequiredService<NiyaCRM.Core.AppInstallation.AppInitialisation.IAppInitialisationService>();
    await initialiser.InitialiseAppAsync();
}

await app.RunAsync();
