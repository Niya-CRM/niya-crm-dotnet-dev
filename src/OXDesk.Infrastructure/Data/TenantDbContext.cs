using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
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

        /// <summary>
        /// Initializes a new instance using typed DbContext options for <see cref="TenantDbContext"/>.
        /// </summary>
        /// <param name="options">Typed options specific to the tenant context.</param>
        /// <param name="serviceProvider">The current service provider.</param>
        /// <param name="currentTenant">Provides the tenant identifier for filters.</param>
        [ActivatorUtilitiesConstructor]
        public TenantDbContext(
            DbContextOptions<TenantDbContext> options,
            IServiceProvider serviceProvider,
            ICurrentTenant currentTenant)
            : this((DbContextOptions)options, serviceProvider, currentTenant)
        {
        }
    }
}
