using Microsoft.AspNetCore.HttpLogging;
using NiyaCRM.Api.Configurations;
using NiyaCRM.Infrastructure.Logging.Serilog;
using NiyaCRM.Infrastructure.Data;
using NiyaCRM.Core.Tenants;
using NiyaCRM.Application.Tenants;
using NiyaCRM.Infrastructure.Data.Tenants;
using NiyaCRM.Core.AuditLogs;
using NiyaCRM.Application.AuditLogs;
using NiyaCRM.Infrastructure.Data.AuditLogs;
using NiyaCRM.Core;
using Serilog;
using System.Reflection;

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

builder.Services.AddControllers();

// For http request context accessing
builder.Services.AddHttpContextAccessor();

// Register ApplicationDbContext with PostgreSQL
builder.Services.AddPostgreSqlDbContext(builder.Configuration);

// Register Tenant Services
builder.Services.AddScoped<ITenantService, TenantService>();
builder.Services.AddScoped<ITenantRepository, TenantRepository>();

// Register AuditLog Services
builder.Services.AddScoped<IAuditLogService, AuditLogService>();
builder.Services.AddScoped<IAuditLogRepository, AuditLogRepository>();

// Register Cache Services
builder.Services.AddMemoryCache();
builder.Services.AddScoped<NiyaCRM.Core.Cache.ICacheRepository, NiyaCRM.Infrastructure.Cache.CacheRepository>();
builder.Services.AddScoped<NiyaCRM.Core.Cache.ICacheService, NiyaCRM.Application.Cache.CacheService>();

// Register Unit of Work
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

// Register Serilog using the extension method
builder.Host.RegisterSerilog();

var app = builder.Build();

// Log all requests and response.
app.UseHttpLogging();

// Streamlines framework logs into a single message per request, including path, method, timings, status code, and exception.
app.UseSerilogRequestLogging();

app.MapControllers();

app.MapGet("/", () => "Hello World!");

await app.RunAsync();
