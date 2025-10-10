using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using OXDesk.Core.Configurations;
using Serilog;
using System;
using System.Threading.Tasks;

namespace OXDesk.Infrastructure.Data
{
    /// <summary>
    /// Extension methods for database context operations.
    /// </summary>
    public static class DbContextExtensions
    {
        /// <summary>
        /// Result class for lock operations.
        /// </summary>
        public class LockResult
        {
            /// <summary>
            /// Gets or sets a value indicating whether the lock was acquired.
            /// </summary>
            public bool Locked { get; set; }
        }

        /// <summary>
        /// Attempts to acquire a PostgreSQL advisory lock.
        /// </summary>
        /// <param name="dbContext">The database context.</param>
        /// <param name="lockId">The lock identifier.</param>
        /// <returns>True if the lock was acquired; otherwise, false.</returns>
        public static async Task<bool> TryAcquireAdvisoryLock(ApplicationDbContext dbContext, long lockId)
        {
            // EF Core expects a column named "Value" for primitive projections; alias accordingly
            var result = await dbContext.Database
                .SqlQueryRaw<bool>("SELECT pg_try_advisory_lock({0}) AS \"Value\"", lockId)
                .SingleAsync();
            return result;
        }


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
                if (true) //environment.IsDevelopment())
                {
                    options.EnableSensitiveDataLogging();
                    options.EnableDetailedErrors();

                    // Log to console with Serilog
                    options.LogTo(
                        message => Serilog.Log.Information(message),
                        new[] { "Microsoft.EntityFrameworkCore.Database.Command" },
                        LogLevel.Information,
                        DbContextLoggerOptions.DefaultWithUtcTime);
                }
            });

            return services;
        }
        
        /// <summary>
        /// Applies database migrations and tenant policies automatically at application startup.
        /// </summary>
        /// <param name="app">The application builder.</param>
        /// <param name="logger">The Serilog logger instance.</param>
        /// <returns>The application builder with migrations applied.</returns>
        public static async Task<IApplicationBuilder> ApplyMigrationsAndTenantPolicies(this IApplicationBuilder app, Serilog.ILogger logger)
        {
            try
            {
                logger.Information("Starting database migration process");
                
                using (var scope = app.ApplicationServices.CreateScope())
                {
                    var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

                    var gotLock = await TryAcquireAdvisoryLock(dbContext, 241022467);
                    
                    if (gotLock)
                    {
                        try
                        {
                            logger.Information("Applying pending database migrations");
                            
                            await dbContext.Database.MigrateAsync();
                            
                            logger.Information("Database migrations applied successfully");
                            
                            logger.Information("Applying tenant policies");
                            
                            string sqlFilePath = Path.Combine(AppContext.BaseDirectory, "Scripts", "ApplyTenantPolicy.sql");
                            var sql = File.ReadAllText(sqlFilePath);
                            
                            await dbContext.Database.ExecuteSqlRawAsync(sql);
                            
                            logger.Information("Tenant policies applied successfully");
                        }
                        finally
                        {
                            await dbContext.Database.ExecuteSqlRawAsync("SELECT pg_advisory_unlock(241022467)");
                        }
                    }
                }
                
                logger.Information("Database initialization completed successfully");
            }
            catch (Exception ex)
            {
                logger.Error(ex, "An error occurred while applying migrations or tenant policies");
                throw; // Re-throw to allow the application to decide how to handle startup failures
            }
            
            return app;
        }
    }
}
