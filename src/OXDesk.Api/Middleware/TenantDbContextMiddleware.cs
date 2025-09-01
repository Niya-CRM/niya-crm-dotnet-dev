using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OXDesk.Infrastructure.Data;

namespace OXDesk.Api.Middleware
{
    public class TenantDbContextMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<TenantDbContextMiddleware> _logger;
        
        public TenantDbContextMiddleware(RequestDelegate next, ILogger<TenantDbContextMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }
        
        public async Task InvokeAsync(HttpContext context, ApplicationDbContext dbContext)
        {
            try
            {
                // Set the PostgreSQL session variable for app.current_tenant_id
                dbContext.SetCurrentTenantIdSessionVariable();
            }
            catch (Exception ex)
            {
                // Log the error but don't stop the request pipeline
                _logger.LogError(ex, "Error setting app.current_tenant_id session variable in database context");
            }
            
            // Continue processing the request
            await _next(context);
        }
    }
    
    // Extension method for easy registration in Startup
    public static class TenantDbContextMiddlewareExtensions
    {
        public static IApplicationBuilder UseTenantDbContext(
            this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<TenantDbContextMiddleware>();
        }
    }
}
