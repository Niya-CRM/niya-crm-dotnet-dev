using System;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using OXDesk.Core.Helpers.Naming;
using OXDesk.Core.Tenants;

namespace OXDesk.Infrastructure.Data
{
    /// <summary>
    /// Application database context for global (non-tenant-specific) entities.
    /// </summary>
    public class ApplicationDbContext : DbContext
    {
        private readonly IServiceProvider? _serviceProvider;

        /// <summary>
        /// Initializes a new instance of the <see cref="ApplicationDbContext" /> class.
        /// </summary>
        /// <param name="options">Typed DbContext options specific to <see cref="ApplicationDbContext"/>.</param>
        /// <param name="serviceProvider">The service provider to resolve services.</param>
        [ActivatorUtilitiesConstructor]
        public ApplicationDbContext(
            DbContextOptions<ApplicationDbContext> options,
            IServiceProvider serviceProvider)
            : base(options)
        {
            _serviceProvider = serviceProvider;
        }

        /// <summary>
        /// Test-only constructor accepting typed DbContext options for <see cref="ApplicationDbContext"/>.
        /// </summary>
        /// <param name="options">Typed DbContext options specific to <see cref="ApplicationDbContext"/>.</param>
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
            _serviceProvider = null;
        }

        /// <summary>
        /// Gets or sets the Tenant DbSet (non-tenant-scoped entity).
        /// </summary>
        public DbSet<Tenant> Tenants { get; set; } = null!;

        /// <summary>
        /// Configures the model for global entities only.
        /// </summary>
        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Apply entity configurations for global entities only
            builder.ApplyConfigurationsFromAssembly(
                Assembly.GetExecutingAssembly(),
                t => t.Namespace != null && t.Namespace.Contains(".Data.Tenants"));

            // Apply snake_case naming convention to all tables, columns, keys and indexes
            foreach (var entity in builder.Model.GetEntityTypes())
            {
                // table names – default snake_case
                var defaultTableName = entity.GetTableName()!.ToSnakeCase();

                entity.SetTableName(defaultTableName);

                // column names
                foreach (var property in entity.GetProperties())
                {
                    property.SetColumnName(property.Name.ToSnakeCase());
                }

                // key names
                foreach (var key in entity.GetKeys())
                {
                    key.SetName(key.GetName()!.ToSnakeCase());
                }

                // index names
                foreach (var index in entity.GetIndexes())
                {
                    index.SetDatabaseName(index.GetDatabaseName()!.ToSnakeCase());
                }
            }
        }

        /// <summary>
        /// Can be overridden by derived contexts (future requirements)
        /// </summary>
        /// <returns>Number of affected rows.</returns>
        public override int SaveChanges()
        {
            return base.SaveChanges();
        }

        /// <summary>
        /// Can be overridden by derived contexts (future requirements)
        /// </summary>
        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            return await base.SaveChangesAsync(cancellationToken);
        }

        
    }
}
