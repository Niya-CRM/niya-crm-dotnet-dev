using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace OXDesk.Application.MultiTenancy;

public static class MultiTenancyExtensions
{
    public static IServiceCollection AddMultiTenancy(this IServiceCollection services)
    {
        services.AddHttpContextAccessor();
        services.AddSingleton<TenantContext>();
        services.AddScoped<ITenantResolutionStrategy, HostTenantResolutionStrategy>();
        services.AddScoped<ITenantService, TenantService>();
        // You must implement and register ITenantRepository in your DI setup
        return services;
    }

    public static IApplicationBuilder UseMultiTenancy(this IApplicationBuilder app)
    {
        return app.UseMiddleware<MultiTenancyMiddleware>();
    }
}
