using System;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using OXDesk.Core.Identity;
using Microsoft.EntityFrameworkCore;
using OXDesk.Core.Helpers.Naming;
using OXDesk.Core.AuditLogs;
using OXDesk.Core.AuditLogs.ChangeHistory;
using OXDesk.Core.Tenants;
using OXDesk.Core.DynamicObjects;
using OXDesk.Core.DynamicObjects.Fields;
using OXDesk.Core.ValueLists;
using OXDesk.Core.AppInstallation;
using OXDesk.Core.Tickets;
using Npgsql;

namespace OXDesk.Infrastructure.Data
{
    /// <summary>
    /// Application database context.
    /// </summary>
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser, ApplicationRole, int, ApplicationUserClaim, ApplicationUserRole, ApplicationUserLogin, ApplicationRoleClaim, ApplicationUserToken>
    {
        private readonly IServiceProvider? _serviceProvider;
        private readonly ICurrentTenant? _currentTenant;

        // Expose current tenant id for EF Core global filters (evaluated per DbContext instance)
        public Guid CurrentTenantId => _currentTenant?.Id
            ?? throw new InvalidOperationException("Tenant context is required but was not provided.");

        /// <summary>
        /// Initializes a new instance of the <see cref="ApplicationDbContext" /> class.
        /// </summary>
        /// <param name="options">The options for the database context.</param>
        /// <param name="serviceProvider">The service provider to resolve services.</param>
        /// <param name="currentTenant">The current tenant accessor.</param>
        public ApplicationDbContext(
            DbContextOptions<ApplicationDbContext> options,
            IServiceProvider serviceProvider,
            ICurrentTenant currentTenant)
            : base(options)
        {
            _serviceProvider = serviceProvider;
            _currentTenant = currentTenant;
        }

        /// <summary>
        /// Test-only convenience constructor allowing creation with just options (e.g., InMemory provider).
        /// Multi-tenant features that rely on ICurrentTenant will be inactive when using this overload.
        /// </summary>
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
            _serviceProvider = null;
            _currentTenant = null;
        }

        /// <summary>
        /// Gets or sets the DbSet.
        /// </summary>
        public DbSet<Tenant> Tenants { get; set; } = null!;
        public DbSet<AuditLog> AuditLogs { get; set; } = null!;
        public DbSet<DynamicObject> DynamicObjects { get; set; } = null!;
        public DbSet<ValueList> ValueLists { get; set; } = null!;
        public DbSet<ValueListItem> ValueListItems { get; set; } = null!;
        public DbSet<ChangeHistoryLog> ChangeHistoryLogs { get; set; } = null!;
        public DbSet<AppInstallationStatus> AppInstallationStatus { get; set; } = null!;
        public DbSet<Permission> Permissions { get; set; } = null!;
        public DbSet<DynamicObjectField> DynamicObjectFields { get; set; } = null!;
        public DbSet<DynamicObjectFieldType> DynamicObjectFieldTypes { get; set; } = null!;
        public DbSet<UserRefreshToken> UserRefreshTokens { get; set; } = null!;
        public DbSet<Ticket> Tickets { get; set; } = null!;
        public DbSet<Status> TicketStatuses { get; set; } = null!;
        public DbSet<Channel> Channels { get; set; } = null!;
        public DbSet<Brand> Brands { get; set; } = null!;
        public DbSet<Priority> Priorities { get; set; } = null!;
        public DbSet<Workflow> Workflows { get; set; } = null!;
        public DbSet<WorkFlowStatus> WorkFlowStatuses { get; set; } = null!;
        public DbSet<WorkflowMapping> WorkflowMappings { get; set; } = null!;

        /// <summary>
        /// Configures the model and customizes Identity table names.
        /// </summary>
        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            
            // Apply all entity configurations from the current assembly
            builder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());

            // Apply snake_case naming convention to all tables, columns, keys and indexes
            foreach (var entity in builder.Model.GetEntityTypes())
            {
                // table names – default snake_case
                var defaultTableName = entity.GetTableName()!.ToSnakeCase();

                // For ASP.NET Identity tables, override to concise names (users, roles, etc.)
                defaultTableName = entity.ClrType switch
                {
                    Type t when t == typeof(ApplicationUser)              => "users",
                    Type t when t == typeof(ApplicationRole)            => "roles",
                    Type t when t == typeof(ApplicationUserRole)        => "user_roles",
                    Type t when t == typeof(ApplicationUserClaim)       => "user_claims",
                    Type t when t == typeof(ApplicationUserLogin)       => "user_logins",
                    Type t when t == typeof(ApplicationRoleClaim)       => "role_claims",
                    Type t when t == typeof(ApplicationUserToken)       => "user_tokens",
                    _                                                     => defaultTableName
                };

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

            // Global query filters for multi-tenancy (strict: require tenant to be present)
            builder.Entity<ApplicationUser>()
                .HasQueryFilter(e => e.TenantId == CurrentTenantId);
            builder.Entity<ApplicationRole>()
                .HasQueryFilter(e => e.TenantId == CurrentTenantId);

            // Core entities with TenantId
            builder.Entity<Permission>()
                .HasQueryFilter(e => e.TenantId == CurrentTenantId);
            builder.Entity<ValueList>()
                .HasQueryFilter(e => e.TenantId == CurrentTenantId);
            builder.Entity<ValueListItem>()
                .HasQueryFilter(e => e.TenantId == CurrentTenantId);
            builder.Entity<AuditLog>()
                .HasQueryFilter(e => e.TenantId == CurrentTenantId);
            builder.Entity<ChangeHistoryLog>()
                .HasQueryFilter(e => e.TenantId == CurrentTenantId);
            builder.Entity<UserRefreshToken>()
                .HasQueryFilter(e => e.TenantId == CurrentTenantId);
            builder.Entity<DynamicObject>()
                .HasQueryFilter(e => e.TenantId == CurrentTenantId);
            builder.Entity<DynamicObjectField>()
                .HasQueryFilter(e => e.TenantId == CurrentTenantId);

            // Ticketing
            builder.Entity<Ticket>()
                .HasQueryFilter(e => e.TenantId == CurrentTenantId);
            builder.Entity<Status>()
                .HasQueryFilter(e => e.TenantId == CurrentTenantId);
            builder.Entity<Channel>()
                .HasQueryFilter(e => e.TenantId == CurrentTenantId);
            builder.Entity<Brand>()
                .HasQueryFilter(e => e.TenantId == CurrentTenantId);
            builder.Entity<Priority>()
                .HasQueryFilter(e => e.TenantId == CurrentTenantId);
            builder.Entity<Workflow>()
                .HasQueryFilter(e => e.TenantId == CurrentTenantId);
            builder.Entity<WorkFlowStatus>()
                .HasQueryFilter(e => e.TenantId == CurrentTenantId);
            builder.Entity<WorkflowMapping>()
                .HasQueryFilter(e => e.TenantId == CurrentTenantId);
        }

        /// <summary>
        /// Automatically sets TenantId for new entities based on ICurrentTenant.
        /// </summary>
        /// <returns>Number of affected rows.</returns>
        public override int SaveChanges()
        {
            ApplyTenantId();
            return base.SaveChanges();
        }

        /// <summary>
        /// Automatically sets TenantId for new entities based on ICurrentTenant.
        /// </summary>
        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            ApplyTenantId();
            return await base.SaveChangesAsync(cancellationToken);
        }

        private void ApplyTenantId()
        {
            var tenantId = _currentTenant?.Id;
            if (!tenantId.HasValue)
            {
                throw new InvalidOperationException("Tenant context is required but was not provided.");
            }

            foreach (var entry in ChangeTracker.Entries())
            {
                if (entry.State != EntityState.Added) continue;

                var tenantProp = entry.Properties.FirstOrDefault(p => string.Equals(p.Metadata.Name, "TenantId", StringComparison.Ordinal));
                if (tenantProp == null) continue;

                if (tenantProp.Metadata.ClrType == typeof(Guid))
                {
                    var current = (Guid?)tenantProp.CurrentValue;
                    if (!current.HasValue || current.Value == Guid.Empty)
                    {
                        tenantProp.CurrentValue = tenantId.Value;
                    }
                }
                else if (tenantProp.Metadata.ClrType == typeof(Guid?))
                {
                    var current = (Guid?)tenantProp.CurrentValue;
                    if (!current.HasValue || current.Value == Guid.Empty)
                    {
                        tenantProp.CurrentValue = tenantId;
                    }
                }
            }
        }
    }
}
