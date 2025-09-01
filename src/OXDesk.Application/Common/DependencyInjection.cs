using Microsoft.Extensions.DependencyInjection;
using OXDesk.Core.Common;

namespace OXDesk.Application.Common
{
    /// <summary>
    /// Extension methods for configuring common application services.
    /// </summary>
    public static class DependencyInjection
    {
        /// <summary>
        /// Adds common application services to the service collection.
        /// </summary>
        /// <param name="services">The service collection.</param>
        /// <returns>The service collection.</returns>
        public static IServiceCollection AddCommonServices(this IServiceCollection services)
        {
            services.AddScoped<ITenantContextService, TenantContextService>();
            
            return services;
        }
    }
}
