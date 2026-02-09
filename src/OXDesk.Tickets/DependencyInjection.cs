using Microsoft.Extensions.DependencyInjection;
using OXDesk.Core.Tickets;
using OXDesk.Tickets.Data;
using OXDesk.Tickets.Factories;
using OXDesk.Tickets.Services;

namespace OXDesk.Tickets;

/// <summary>
/// Extension methods for setting up ticket services.
/// </summary>
public static class DependencyInjection
{
    /// <summary>
    /// Adds ticket-related services to the service collection.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddTickets(this IServiceCollection services)
    {
        services.AddScoped<IBusinessHoursRepository, BusinessHoursRepository>();
        services.AddScoped<ICustomBusinessHoursRepository, CustomBusinessHoursRepository>();
        services.AddScoped<IHolidayRepository, HolidayRepository>();
        services.AddScoped<IBusinessHoursService, BusinessHoursService>();
        services.AddScoped<IBusinessHoursFactory, BusinessHoursFactory>();
        return services;
    }
}
