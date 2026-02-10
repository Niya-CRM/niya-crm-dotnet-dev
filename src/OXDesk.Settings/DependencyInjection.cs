using Microsoft.Extensions.DependencyInjection;
using OXDesk.Core.Settings;
using OXDesk.Settings.Data;
using OXDesk.Settings.Factories;
using OXDesk.Settings.Services;

namespace OXDesk.Settings;

/// <summary>
/// Extension methods for setting up settings services.
/// </summary>
public static class DependencyInjection
{
    /// <summary>
    /// Adds settings-related services to the service collection.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddSettings(this IServiceCollection services)
    {
        services.AddScoped<ISettingRepository, SettingRepository>();
        services.AddScoped<ISettingService, SettingService>();
        services.AddScoped<ISettingFactory, SettingFactory>();
        return services;
    }
}
