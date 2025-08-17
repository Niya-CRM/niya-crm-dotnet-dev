using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.HttpLogging;
using Microsoft.AspNetCore.Identity;
using OXDesk.Core.Identity;
using Microsoft.IdentityModel.Tokens;
using OXDesk.Api.Configurations;
using OXDesk.Infrastructure.Logging.Serilog;
using OXDesk.Infrastructure.Data;
using OXDesk.Core.Tenants;
using OXDesk.Application.Tenants;
using FluentValidation;
using OXDesk.Application.Tenants.Validators;
using OXDesk.Api.Validators.Tenants;
using OXDesk.Infrastructure.Data.Tenants;
using OXDesk.Core.AuditLogs;
using OXDesk.Application.AuditLogs;
using OXDesk.Infrastructure.Data.AuditLogs;
using OXDesk.Core.AuditLogs.ChangeHistory;
using OXDesk.Application.AuditLogs.ChangeHistory;
using OXDesk.Infrastructure.Data.AuditLogs.ChangeHistory;
using OXDesk.Core.DynamicObjects;
using OXDesk.Application.DynamicObjects;
using OXDesk.Infrastructure.Data.DynamicObjects;
using OXDesk.Application.Identity;
using OXDesk.Infrastructure.Data.Identity;
using OXDesk.Core;
using OXDesk.Core.ValueLists;
using OXDesk.Application.ValueLists;
using OXDesk.Infrastructure.Data.ValueLists;
using Serilog;
using System.Reflection;
using System.Text;
using OXDesk.Core.Tenants.DTOs;
using OXDesk.Api.Middleware;
using OXDesk.Api.Helpers;
using OXDesk.Core.Auth.Constants;
using OXDesk.Core.Common;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using OXDesk.Api.Conventions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.OpenApi.Models;
using OXDesk.AppInstallation;
using Swashbuckle.AspNetCore.SwaggerGen;
using OXDesk.Core.Identity.DTOs;
using OXDesk.Application.Identity.Validators;

try
{
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
// Register validator for ActivateDeactivateUserRequest
builder.Services.AddScoped<IValidator<ActivateDeactivateUserRequest>, ActivateDeactivateUserRequestValidator>();


// For http request context accessing
builder.Services.AddHttpContextAccessor();

// Register ApplicationDbContext with PostgreSQL
builder.Services.AddPostgreSqlDbContext(builder.Configuration, builder.Environment);

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

builder.Services.AddScoped<OXDesk.Core.Identity.IPermissionRepository, OXDesk.Infrastructure.Data.Identity.PermissionRepository>();
builder.Services.AddScoped<OXDesk.Core.Identity.IPermissionService, OXDesk.Application.Identity.PermissionService>();
// Register Role Service
builder.Services.AddScoped<OXDesk.Core.Identity.IRoleClaimRepository, OXDesk.Infrastructure.Data.Identity.RoleClaimRepository>();
builder.Services.AddScoped<OXDesk.Core.Identity.IRoleService, OXDesk.Application.Identity.RoleService>();

// Add Authorization policies with global fallback policy using AuthorizationBuilder
var authBuilder = builder.Services.AddAuthorizationBuilder();

// This makes all endpoints require authentication by default
authBuilder.SetFallbackPolicy(new AuthorizationPolicyBuilder()
    .RequireAuthenticatedUser()
    .Build());

// Permission-based policies
authBuilder.AddPolicy(CommonConstant.PermissionNames.SysSetupRead, policy =>
    policy.RequireClaim("permission", CommonConstant.PermissionNames.SysSetupRead));
authBuilder.AddPolicy(CommonConstant.PermissionNames.SysSetupWrite, policy =>
    policy.RequireClaim("permission", CommonConstant.PermissionNames.SysSetupWrite));

// Register Tenant Services
builder.Services.AddScoped<ITenantService, TenantService>();
builder.Services.AddScoped<ITenantRepository, TenantRepository>();

// Register AuditLog Services
builder.Services.AddScoped<IAuditLogService, AuditLogService>();
builder.Services.AddScoped<IAuditLogRepository, AuditLogRepository>();

// Register ChangeHistoryLog Services
builder.Services.AddScoped<IChangeHistoryLogService, ChangeHistoryLogService>();
builder.Services.AddScoped<IChangeHistoryLogRepository, ChangeHistoryLogRepository>();

// Register DynamicObject Services
builder.Services.AddScoped<IDynamicObjectService, DynamicObjectService>();
builder.Services.AddScoped<IDynamicObjectRepository, DynamicObjectRepository>();

// Register DynamicObject Field Type Services
builder.Services.AddScoped<OXDesk.Core.DynamicObjects.Fields.IDynamicObjectFieldRepository, OXDesk.Infrastructure.Data.DynamicObjects.Fields.DynamicObjectFieldRepository>();
builder.Services.AddScoped<OXDesk.Core.DynamicObjects.Fields.IDynamicObjectFieldService, OXDesk.Application.DynamicObjects.Fields.DynamicObjectFieldService>();

// Register User Services
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IUserService, UserService>();

// Register Refresh Token Repository
builder.Services.AddScoped<OXDesk.Core.Identity.IRefreshTokenRepository, OXDesk.Infrastructure.Data.Identity.RefreshTokenRepository>();

// Register ReferenceData Services
builder.Services.AddScoped<OXDesk.Core.Referentials.ICountryRepository, OXDesk.Infrastructure.Data.ReferenceData.CountryRepository>();
builder.Services.AddScoped<OXDesk.Core.Referentials.IReferenceDataService, OXDesk.Application.Referentials.ReferenceDataService>();

// Register ValueList Services
builder.Services.AddScoped<IValueListRepository, ValueListRepository>();
builder.Services.AddScoped<IValueListService, ValueListService>();
builder.Services.AddScoped<IValueListItemRepository, ValueListItemRepository>();
builder.Services.AddScoped<IValueListItemService, ValueListItemService>();

// Register Cache Services
builder.Services.AddMemoryCache();

// Register FluentValidation
builder.Services.AddScoped<IValidator<CreateTenantRequest>, CreateTenantRequestValidator>();
builder.Services.AddScoped<IValidator<CreatePermissionRequest>, CreatePermissionRequestValidator>();
builder.Services.AddScoped<IValidator<UpdatePermissionRequest>, UpdatePermissionRequestValidator>();
builder.Services.AddScoped<IValidator<CreateRoleRequest>, CreateRoleRequestValidator>();
builder.Services.AddScoped<IValidator<UpdateRoleRequest>, UpdateRoleRequestValidator>();
builder.Services.AddScoped<OXDesk.Core.Cache.ICacheRepository, OXDesk.Infrastructure.Cache.CacheRepository>();
builder.Services.AddScoped<OXDesk.Core.Cache.ICacheService, OXDesk.Application.Cache.CacheService>();

// Register Unit of Work
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

// Register JwtHelper
builder.Services.AddScoped<JwtHelper>();

// Register AppInstallation Services
builder.Services.AddAppInstallation();

// Register Swagger/OpenAPI
builder.Services.AddSwaggerServices(builder.Configuration);

// Health Checks
builder.Services.AddHealthChecks()
    .AddCheck("self", () => HealthCheckResult.Healthy(), tags: CommonConstant.HealthCheck.ServiceTags)
    .AddCheck<DatabaseHealthCheck>("database_connection", tags: CommonConstant.HealthCheck.DatabaseTags);

// Register the database health check as a service
builder.Services.AddScoped<DatabaseHealthCheck>();

// Register Serilog using the extension method
builder.Host.RegisterSerilog();

builder.Host.UseDefaultServiceProvider(options =>
{
    options.ValidateScopes = true;
    options.ValidateOnBuild = true;
});

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
    var initialiser = scope.ServiceProvider.GetRequiredService<OXDesk.Core.AppInstallation.AppInitialisation.IAppInitialisationService>();
    await initialiser.InitialiseAppAsync();
}

await app.RunAsync();
}
catch (Exception ex)
{
    Console.WriteLine("Startup error: " + ex);
    throw;
}