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
    /// <summary>
    /// Middleware that extracts and sets the tenant context from JWT claims or X-Forwarded-Host header.
    /// </summary>
    public class TenantMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<TenantMiddleware> _logger;
        
        /// <summary>
        /// The key used to store tenant ID in HttpContext.Items and JWT claims.
        /// </summary>
        public const string TenantIdKey = "tenant_id";
        
        /// <summary>
        /// The key used to store tenant schema in HttpContext.Items and JWT claims.
        /// </summary>
        public const string TenantSchemaKey = "tenant_schema";
        
        /// <summary>
        /// Initializes a new instance of the <see cref="TenantMiddleware"/> class.
        /// </summary>
        /// <param name="next">The next middleware in the pipeline.</param>
        /// <param name="logger">The logger.</param>
        public TenantMiddleware(RequestDelegate next, ILogger<TenantMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }
        
        /// <summary>
        /// Invokes the middleware to extract and set tenant context.
        /// </summary>
        /// <param name="context">The HTTP context.</param>
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
                        
                        // Fetch tenant schema from database (not from JWT for security)
                        try
                        {
                            var tenantService = context.RequestServices.GetRequiredService<ITenantService>();
                            var tenant = await tenantService.GetTenantByIdAsync(tenantId);
                            
                            string? tenantSchema = tenant?.Schema;
                            if (!string.IsNullOrEmpty(tenantSchema))
                            {
                                context.Items[TenantSchemaKey] = tenantSchema;
                            }
                            
                            // Set the current tenant via ICurrentTenant
                            var currentTenant = context.RequestServices.GetService(typeof(ICurrentTenant)) as ICurrentTenant;
                            currentTenant?.Change(tenantId, tenantSchema);
                            
                            _logger.LogDebug("Tenant ID {TenantId} and Schema {Schema} added to HttpContext", tenantId, tenantSchema ?? "public");
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, "Error fetching tenant schema for tenant {TenantId}", tenantId);
                            
                            // Set tenant without schema as fallback
                            var currentTenant = context.RequestServices.GetService(typeof(ICurrentTenant)) as ICurrentTenant;
                            currentTenant?.Change(tenantId, null);
                        }
                    }
                    else
                    {
                        _logger.LogCritical("No valid tenant_id found in the user claims");
                    }
                }
                else
                {
                    // Fallback for unauthenticated requests: resolve tenant from X-Forwarded-Host or current Host
                    var forwardedHostHeader = context.Request.Headers["X-Forwarded-Host"].FirstOrDefault();
                    string? hostToResolve = null;

                    if (!string.IsNullOrWhiteSpace(forwardedHostHeader))
                    {
                        // Some proxies may send multiple hosts separated by comma, take the first
                        var firstHost = forwardedHostHeader.Split(',').FirstOrDefault()?.Trim();
                        if (!string.IsNullOrWhiteSpace(firstHost))
                        {
                            // Strip port if present (host:port)
                            hostToResolve = firstHost.Split(':').FirstOrDefault() ?? firstHost;
                        }
                    }
                    else
                    {
                        // Fallback to current request host if X-Forwarded-Host is not present
                        var currentHost = context.Request.Host.Host;
                        if (!string.IsNullOrWhiteSpace(currentHost))
                        {
                            hostToResolve = currentHost;
                        }
                    }

                    if (!string.IsNullOrWhiteSpace(hostToResolve))
                    {
                        try
                        {
                            var tenantService = context.RequestServices.GetRequiredService<ITenantService>();
                            var tenant = await tenantService.GetTenantByHostAsync(hostToResolve);
                            if (tenant != null)
                            {
                                context.Items[TenantIdKey] = tenant.Id;
                                
                                if (!string.IsNullOrEmpty(tenant.Schema))
                                {
                                    context.Items[TenantSchemaKey] = tenant.Schema;
                                }

                                var currentTenant = context.RequestServices.GetService(typeof(ICurrentTenant)) as ICurrentTenant;
                                currentTenant?.Change(tenant.Id, tenant.Schema);

                                _logger.LogDebug("Resolved tenant {TenantId} with schema {Schema} from host: {Host}", tenant.Id, tenant.Schema ?? "public", hostToResolve);
                            }
                            else
                            {
                                _logger.LogWarning("No tenant found for host: {Host}", hostToResolve);
                            }
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, "Error resolving tenant from host header: {Host}", hostToResolve);
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
    
    /// <summary>
    /// Extension methods for registering TenantMiddleware.
    /// </summary>
    public static class TenantMiddlewareExtensions
    {
        /// <summary>
        /// Adds the TenantMiddleware to the application pipeline.
        /// </summary>
        /// <param name="builder">The application builder.</param>
        /// <returns>The application builder for chaining.</returns>
        public static IApplicationBuilder UseTenantMiddleware(
            this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<TenantMiddleware>();
        }
    }
}

