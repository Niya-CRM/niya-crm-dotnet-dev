using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OXDesk.Core.Common;
using OXDesk.Core.Tenants;

namespace OXDesk.Api.Middleware
{
    public class TenantMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<TenantMiddleware> _logger;
        public const string TenantIdKey = "tenant_id";
        
        public TenantMiddleware(RequestDelegate next, ILogger<TenantMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }
        
        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                // Extract tenant_id from the claims if the user is authenticated
                if (context.User?.Identity?.IsAuthenticated == true)
                {
                    var tenantClaim = context.User.Claims.FirstOrDefault(c => c.Type == TenantIdKey);
                    
                    if (tenantClaim != null && !string.IsNullOrEmpty(tenantClaim.Value) && 
                        Guid.TryParse(tenantClaim.Value, out Guid tenantId))
                    {
                        // Add tenant_id to HttpContext.Items for use throughout the request
                        context.Items[TenantIdKey] = tenantId;
                        
                        // Set the current tenant via ICurrentTenant
                        var currentTenant = context.RequestServices.GetService(typeof(ICurrentTenant)) as ICurrentTenant;
                        currentTenant?.Change(tenantId);
                        
                        _logger.LogDebug("Tenant ID {TenantId} added to HttpContext", tenantId);
                    }
                    else
                    {
                        _logger.LogCritical("No valid tenant_id found in the user claims");
                    }
                }
                else
                {
                    // Fallback for unauthenticated requests: resolve tenant from X-Forwarded-Host
                    var forwardedHostHeader = context.Request.Headers["X-Forwarded-Host"].FirstOrDefault();
                    if (!string.IsNullOrWhiteSpace(forwardedHostHeader))
                    {
                        // Some proxies may send multiple hosts separated by comma, take the first
                        var firstHost = forwardedHostHeader.Split(',').FirstOrDefault()?.Trim();
                        if (!string.IsNullOrWhiteSpace(firstHost))
                        {
                            // Strip port if present (host:port)
                            var hostWithoutPort = firstHost.Split(':').FirstOrDefault() ?? firstHost;

                            try
                            {
                                var tenantService = context.RequestServices.GetRequiredService<ITenantService>();
                                var tenant = await tenantService.GetTenantByHostAsync(hostWithoutPort);
                                if (tenant != null)
                                {
                                    context.Items[TenantIdKey] = tenant.Id;

                                    var currentTenant = context.RequestServices.GetService(typeof(ICurrentTenant)) as ICurrentTenant;
                                    currentTenant?.Change(tenant.Id);

                                    _logger.LogDebug("Resolved tenant {TenantId} from X-Forwarded-Host: {Host}", tenant.Id, hostWithoutPort);
                                }
                                else
                                {
                                    _logger.LogWarning("No tenant found for X-Forwarded-Host: {Host}", hostWithoutPort);
                                }
                            }
                            catch (Exception ex)
                            {
                                _logger.LogError(ex, "Error resolving tenant from X-Forwarded-Host header");
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // Log the error but don't stop the request pipeline
                _logger.LogError(ex, "Error extracting tenant_id from claims");
            }
            
            // Continue processing the request
            await _next(context);
        }
    }
    
    // Extension method for easy registration in Startup
    public static class TenantMiddlewareExtensions
    {
        public static IApplicationBuilder UseTenantMiddleware(
            this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<TenantMiddleware>();
        }
    }
}

