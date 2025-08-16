using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using OXDesk.Core.Configurations;

namespace OXDesk.Infrastructure.Data
{
    public static class DbContextExtensions
    {
        /// <summary>
        /// Adds the PostgreSQL database context to the service collection.
        /// </summary>
        /// <param name="services">The service collection to add the database context to.</param>
        /// <param name="configuration">The configuration to use for the database context.</param>
        /// <returns>The service collection with the database context added.</returns>
        public static IServiceCollection AddPostgreSqlDbContext(this IServiceCollection services, IConfiguration configuration, IHostEnvironment environment)
        {
            // Bind PostgreSQL settings from postgresql.json
            var postgreSqlSettings = new PostgreSqlSettings();
            configuration.GetSection("PostgreSQL").Bind(postgreSqlSettings);

            _ = services.AddDbContext<ApplicationDbContext>(options =>
            {
                options.UseNpgsql(postgreSqlSettings.ConnectionString);

                // Only enable detailed logging in development environment
                if (environment.IsDevelopment())
                {
                    options.EnableSensitiveDataLogging();
                    options.EnableDetailedErrors();

                    // Log to console with Serilog
                    options.LogTo(
                        message => Serilog.Log.Debug(message),
                        new[] { "Microsoft.EntityFrameworkCore.Database.Command" },
                        LogLevel.Information,
                        DbContextLoggerOptions.DefaultWithUtcTime);
                }
            });

            return services;
        }
    }
}
