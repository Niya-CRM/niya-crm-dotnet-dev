using Microsoft.Extensions.Diagnostics.HealthChecks;
using OXDesk.DbContext.Data;

namespace OXDesk.Api.Helpers
{
    /// <summary>
    /// Health check for database connectivity
    /// </summary>
    public class DatabaseHealthCheck : IHealthCheck
    {
        private readonly ApplicationDbContext _dbContext;

        /// <summary>
        /// Initializes a new instance of the <see cref="DatabaseHealthCheck"/> class.
        /// </summary>
        /// <param name="dbContext">The database context.</param>
        public DatabaseHealthCheck(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        /// <inheritdoc/>
        public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            try
            {
                // Simple check if database connection is available
                bool canConnect = await _dbContext.Database.CanConnectAsync(cancellationToken);
                
                if (canConnect)
                {
                    return HealthCheckResult.Healthy("Database connection is healthy");
                }
                
                return HealthCheckResult.Unhealthy("Cannot connect to database");
            }
            catch (Exception ex)
            {
                return HealthCheckResult.Unhealthy($"Database connection error: {ex.Message}");
            }
        }
    }
}
