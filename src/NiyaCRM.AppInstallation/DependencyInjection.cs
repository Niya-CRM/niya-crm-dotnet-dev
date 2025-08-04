using Microsoft.Extensions.DependencyInjection;
using NiyaCRM.Core.AppInstallation.AppInitialisation;
using NiyaCRM.Core.AppInstallation.AppSetup;
using NiyaCRM.AppInstallation.Services;

namespace NiyaCRM.AppInstallation
{
    /// <summary>
    /// Extension methods for setting up application installation services
    /// </summary>
    public static class DependencyInjection
    {
        /// <summary>
        /// Adds application installation services to the service collection
        /// </summary>
        /// <param name="services">The service collection</param>
        /// <returns>The service collection for chaining</returns>
        public static IServiceCollection AddAppInstallation(this IServiceCollection services)
        {
            // Register services
            services.AddScoped<IAppInitialisationService, AppInitialisationService>();
            services.AddScoped<IAppSetupService, AppSetupService>();
            return services;
        }
    }
}
