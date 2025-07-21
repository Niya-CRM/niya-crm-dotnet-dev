using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NiyaCRM.Core.Configurations;

namespace NiyaCRM.Infrastructure.Data
{
    public static class DbContextExtensions
    {
        /// <summary>
        /// Adds the PostgreSQL database context to the service collection.
        /// </summary>
        /// <param name="services">The service collection to add the database context to.</param>
        /// <param name="configuration">The configuration to use for the database context.</param>
        /// <returns>The service collection with the database context added.</returns>
        public static IServiceCollection AddPostgreSqlDbContext(this IServiceCollection services, IConfiguration configuration)
        {
            // Bind PostgreSQL settings from postgresql.json
            var postgreSqlSettings = new PostgreSqlSettings();
            configuration.GetSection("PostgreSQL").Bind(postgreSqlSettings);

            services.AddDbContext<ApplicationDbContext>(options =>
            {
                options.UseNpgsql(postgreSqlSettings.ConnectionString);
                options.EnableSensitiveDataLogging();
                options.EnableDetailedErrors();
            });

            return services;
        }
    }
}
