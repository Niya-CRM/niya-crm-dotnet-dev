using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace OXDesk.Application.MultiTenancy;

public class MultiTenancyMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<MultiTenancyMiddleware> _logger;

    public MultiTenancyMiddleware(RequestDelegate next, ILogger<MultiTenancyMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(
        HttpContext context,
        ITenantService tenantService,
        ITenantResolutionStrategy tenantResolutionStrategy)
    {
        var tenantIdentifier = tenantResolutionStrategy.GetTenantIdentifier();
        if (!await tenantService.SetCurrentTenantAsync(tenantIdentifier))
        {
            _logger.LogWarning("Failed to resolve tenant for identifier: {Identifier}", tenantIdentifier);
            context.Response.StatusCode = StatusCodes.Status400BadRequest;
            await context.Response.WriteAsJsonAsync(new { error = "Invalid tenant" });
            return;
        }
        await _next(context);
    }
}
