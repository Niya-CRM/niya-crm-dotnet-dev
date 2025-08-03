using Microsoft.Extensions.DependencyInjection;
using NiyaCRM.AppInstallation.DataSeeding;
using NiyaCRM.Core.AppInstallation.AppInitialisation;
using NiyaCRM.Core.AppInstallation.AppSetup;
using NiyaCRM.AppInstallation.Services;
using NiyaCRM.AppInstallation.UpgradeScripts;

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
            //services.AddScoped<IAppInstallationService, AppInstallationService>();
            services.AddScoped<IAppInitialisationService, AppInitialisationService>();
            services.AddScoped<IAppSetupService, AppSetupService>();
            //services.AddScoped<DataSeeder>();
            //services.AddScoped<VersionManager>();
            return services;
        }
    }
}
