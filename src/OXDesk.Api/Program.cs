using FluentValidation;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.HttpLogging;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.Extensions.DependencyInjection;

using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.OpenApi.Models;
using OpenIddict.Validation.AspNetCore;
using Serilog;
using Swashbuckle.AspNetCore.SwaggerGen;

using OXDesk.Api.Configurations;
using OXDesk.Api.Conventions;
using OXDesk.Api.Extension;
using OXDesk.Api.Factories.AuditLogs;
using OXDesk.Api.Factories.DynamicObjects;
using OXDesk.Api.Factories.Identity;
using OXDesk.Api.Helpers;
using OXDesk.Api.Middleware;
using OXDesk.Application.DynamicObjects;
using OXDesk.Application.DynamicObjects.Fields;
using OXDesk.Application.Identity;
using OXDesk.Application.ValueLists;
using OXDesk.AppInstallation;
using OXDesk.Core;
using OXDesk.Core.AuditLogs;
using OXDesk.Core.AuditLogs.ChangeHistory;
using OXDesk.Core.Auth.Constants;
using OXDesk.Core.Cache;
using OXDesk.Core.Common;
using OXDesk.Core.DynamicObjects;
using OXDesk.Core.DynamicObjects.Fields;
using OXDesk.Core.Identity;
using OXDesk.Core.Identity.DTOs;
using OXDesk.Core.Tenants;
using OXDesk.Core.Tenants.DTOs;
using OXDesk.Core.ValueLists;
using OXDesk.DbContext.Data;
using OXDesk.Infrastructure.Cache;
using OXDesk.Infrastructure.Data;
using OXDesk.Infrastructure.Data.AuditLogs;
using OXDesk.Infrastructure.Data.AuditLogs.ChangeHistory;
using OXDesk.Infrastructure.Data.DynamicObjects;
using OXDesk.Infrastructure.Data.DynamicObjects.Fields;
using OXDesk.Infrastructure.Data.Identity;
using OXDesk.Infrastructure.Data.ValueLists;
using OXDesk.Infrastructure.Logging.Serilog;
using OXDesk.Infrastructure.Redaction;
using OXDesk.Tenant.Data;
using OXDesk.Tenant.Factories;
using OXDesk.Tenant.Services;
using OXDesk.Identity.Data;
using OXDesk.Identity.Services;
using OXDesk.Identity.Factories;
using OXDesk.Identity.Extensions;
using OXDesk.Shared.Services;
using OXDesk.Shared.Common;

try
{
    var builder = WebApplication.CreateBuilder(args);

    builder.AddConfigurations();

    var cookiePrefix = builder.ConfigureDataProtectionAndGetCookiePrefix();

    Log.Information("Server Booting Up...");

    builder.Services
        .AddControllersWithViews(options =>
        {
            // Add a route convention to prefix all controller routes with 'api'
            // except for controllers that should be served directly
            options.Conventions.Add(new ApiControllerRouteConvention());
        });

    builder.Services.AddAntiforgery(options =>
    {
        options.Cookie.Name = $"{cookiePrefix}.Antiforgery";
    });

    // Register data redaction for sensitive & personal data
    builder.Services.AddOxDeskRedaction();

    // Register all FluentValidation validators via extension method
    builder.Services.AddOxDeskValidators();

    // For http request context accessing
    builder.Services.AddHttpContextAccessor();

    // Register current tenant holder (scoped per request)
    builder.Services.AddScoped<OXDesk.Core.Tenants.ICurrentTenant, OXDesk.Shared.Common.CurrentTenant>();

    // Register current user holder (scoped per request)
    builder.Services.AddScoped<OXDesk.Core.Identity.ICurrentUser, OXDesk.Shared.Common.CurrentUser>();

    // Register ApplicationDbContext with PostgreSQL
    builder.Services.AddPostgreSqlDbContext(builder.Configuration, builder.Environment);

    builder.ConfigureIdentityAndCookies(cookiePrefix);

    // Roles and Permissions
    builder.Services.AddScoped<IPermissionRepository, PermissionRepository>();
    builder.Services.AddScoped<IPermissionService, PermissionService>();
    builder.Services.AddScoped<IPermissionFactory, PermissionsFactory>();
    builder.Services.AddScoped<IRoleClaimRepository, RoleClaimRepository>();
    builder.Services.AddScoped<IRoleService, RoleService>();
    builder.Services.AddScoped<IRoleFactory, RolesFactory>();

    // Register User Services (from OXDesk.Identity)
    builder.Services.AddScoped<IUserRepository, OXDesk.Identity.Data.UserRepository>();
    builder.Services.AddScoped<IUserService, OXDesk.Identity.Services.UserService>();
    builder.Services.AddScoped<IUserFactory, OXDesk.Identity.Factories.UserFactory>();

    // Add Authorization policies with global fallback policy using AuthorizationBuilder
    var authBuilder = builder.Services.AddAuthorizationBuilder();

    // This makes all endpoints require authentication by default
    authBuilder.SetFallbackPolicy(new AuthorizationPolicyBuilder()
        .AddAuthenticationSchemes(
            IdentityConstants.ApplicationScheme,
            OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme)
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
    builder.Services.AddScoped<OXDesk.Core.Tenants.ITenantFactory, TenantFactory>();

    // Register Correlation ID Accessor
    builder.Services.AddScoped<ICorrelationIdAccessor, OXDesk.Application.Common.CorrelationIdAccessor>();

    // Register AuditLog Services
    builder.Services.AddScoped<IAuditLogService, AuditLogService>();
    builder.Services.AddScoped<IAuditLogRepository, AuditLogRepository>();
    builder.Services.AddScoped<IAuditLogFactory, AuditLogFactory>();

    // Cache Services
    builder.Services.AddScoped<ICacheRepository, CacheRepository>();
    builder.Services.AddScoped<ICacheService, CacheService>();
    builder.Services.AddMemoryCache();

    // ChangeHistoryLog Services
    builder.Services.AddScoped<IChangeHistoryLogService, ChangeHistoryLogService>();
    builder.Services.AddScoped<IChangeHistoryLogRepository, ChangeHistoryLogRepository>();
    builder.Services.AddScoped<IChangeHistoryLogFactory, ChangeHistoryLogFactory>();

    // DynamicObject Services
    builder.Services.AddScoped<IDynamicObjectService, DynamicObjectService>();
    builder.Services.AddScoped<IDynamicObjectRepository, DynamicObjectRepository>();
    builder.Services.AddScoped<IDynamicObjectFactory, DynamicObjectFactory>();

    // DynamicObject Field Type Services
    builder.Services.AddScoped<IDynamicObjectFieldRepository, DynamicObjectFieldRepository>();
    builder.Services.AddScoped<IDynamicObjectFieldService, DynamicObjectFieldService>();
    builder.Services.AddScoped<IDynamicObjectFieldFactory, DynamicObjectFieldFactory>();

    // Refresh Token Repository (from OXDesk.Identity)
    builder.Services.AddScoped<IUserRefreshTokenRepository, OXDesk.Identity.Data.UserRefreshTokenRepository>();

    // ValueList Services
    builder.Services.AddScoped<IValueListRepository, ValueListRepository>();
    builder.Services.AddScoped<IValueListService, ValueListService>();
    builder.Services.AddScoped<IValueListItemRepository, ValueListItemRepository>();
    builder.Services.AddScoped<IValueListItemService, ValueListItemService>();

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

    // Apply database migrations and tenant policies automatically
    await app.ApplyMigrationsAndTenantPolicies(Log.Logger);

    // Configure Swagger and Swagger UI
    app.UseSwaggerMiddleware(app.Environment);

    app.UseStaticFiles();

    // Add CorrelationId to response headers for all requests
    app.UseMiddleware<CorrelationIdMiddleware>();

    // Add Domain to log context for all requests
    app.UseMiddleware<DomainMiddleware>();

    // Log requests/responses 
    app.UseHttpLogging();

    // Add JWT cookie authentication middleware
    app.UseJwtCookieAuthentication();

    // Enable authentication & authorization
    app.UseAuthentication();

    // Add tenant middleware to extract tenant_id from JWT token
    app.UseTenantMiddleware();

    // Populate current user from JWT when authenticated
    app.UseCurrentUserMiddleware();

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

    // Seed OpenIddict applications
    await app.SeedOpenIddictApplicationsAsync();

    await app.RunAsync();
}
catch (Exception ex)
{
    Console.WriteLine("Startup error: " + ex);
    throw;
}