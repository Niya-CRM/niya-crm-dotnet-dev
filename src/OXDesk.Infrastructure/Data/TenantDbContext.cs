using System;
using Microsoft.EntityFrameworkCore;
using OXDesk.Core.Tenants;

namespace OXDesk.Infrastructure.Data
{
    /// <summary>
    /// Database context dedicated to tenant-scoped entities (tables with tenant_id).
    /// </summary>
    public class TenantDbContext : ApplicationDbContext
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TenantDbContext"/> class.
        /// </summary>
        /// <param name="options">The context configuration options.</param>
        /// <param name="serviceProvider">The current service provider.</param>
        /// <param name="currentTenant">Provides the tenant identifier for filters.</param>
        public TenantDbContext(
            DbContextOptions options,
            IServiceProvider serviceProvider,
            ICurrentTenant currentTenant)
            : base(options, serviceProvider, currentTenant)
        {
        }
    }
}
