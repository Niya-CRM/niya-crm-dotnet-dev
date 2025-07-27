using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.HttpLogging;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using NiyaCRM.Api.Configurations;
using NiyaCRM.Infrastructure.Logging.Serilog;
using NiyaCRM.Infrastructure.Data;
using NiyaCRM.Core.Tenants;
using NiyaCRM.Application.Tenants;
using FluentValidation;
using NiyaCRM.Application.Tenants.Validators;
using NiyaCRM.Infrastructure.Data.Tenants;
using NiyaCRM.Core.AuditLogs;
using NiyaCRM.Application.AuditLogs;
using NiyaCRM.Infrastructure.Data.AuditLogs;
using NiyaCRM.Core;
using NiyaCRM.Core.Identity;
using Serilog;
using System.Reflection;
using System.Text;
using NiyaCRM.Core.Tenants.DTOs;
using NiyaCRM.Api.Middleware;

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

builder.Services.AddControllersWithViews();

// Register validator for ActivateDeactivateTenantRequest
builder.Services.AddScoped<IValidator<ActivateDeactivateTenantRequest>, ActivateDeactivateTenantRequestValidator>();


// For http request context accessing
builder.Services.AddHttpContextAccessor();

// Register ApplicationDbContext with PostgreSQL
builder.Services.AddPostgreSqlDbContext(builder.Configuration);

// Configure Identity
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options => {
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
        ValidIssuer = builder.Configuration["JWT:Issuer"],
        ValidAudience = builder.Configuration["JWT:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["JWT:Secret"] ?? "defaultSecretKeyWhichShouldBeReplaced"))
    };
});

// Add Authorization policies
builder.Services.AddAuthorization();

// Register Tenant Services
builder.Services.AddScoped<ITenantService, TenantService>();
builder.Services.AddScoped<ITenantRepository, TenantRepository>();

// Register AuditLog Services
builder.Services.AddScoped<IAuditLogService, AuditLogService>();
builder.Services.AddScoped<IAuditLogRepository, AuditLogRepository>();

// Register Onboarding Services
builder.Services.AddScoped<NiyaCRM.Core.Onboarding.IOnboardingService, NiyaCRM.Application.Onboarding.OnboardingService>();

// Register Cache Services
builder.Services.AddMemoryCache();
builder.Services.AddScoped<NiyaCRM.Core.Cache.ICacheRepository, NiyaCRM.Infrastructure.Cache.CacheRepository>();
builder.Services.AddScoped<NiyaCRM.Core.Cache.ICacheService, NiyaCRM.Application.Cache.CacheService>();

// Register Unit of Work
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

// Register Serilog using the extension method
builder.Host.RegisterSerilog();

var app = builder.Build();

// Add CorrelationId to response headers for all requests
app.UseMiddleware<CorrelationIdMiddleware>();

// Add Domain to log context for all requests
app.UseMiddleware<DomainMiddleware>();

// Log all requests and response.
app.UseHttpLogging();

// Enable authentication & authorization
app.UseAuthentication();
app.UseAuthorization();

// Streamlines framework logs into a single message per request, including path, method, timings, status code, and exception.
app.UseSerilogRequestLogging();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapGet("/", () => "Hello World!");

await app.RunAsync();
