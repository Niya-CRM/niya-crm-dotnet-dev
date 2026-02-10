using System;
using System.Reflection;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OpenIddict.EntityFrameworkCore;
using OXDesk.Core.AppInstallation;
using OXDesk.Core.AuditLogs;
using OXDesk.Core.AuditLogs.ChangeHistory;
using OXDesk.Core.Common;
using OXDesk.Core.DynamicObjects;
using OXDesk.Core.DynamicObjects.Fields;
using OXDesk.Core.FormTypes;
using OXDesk.Core.Helpers.Naming;
using OXDesk.Core.Identity;
using OXDesk.Core.Tenants;
using OXDesk.Core.Tickets;
using OXDesk.Core.ValueLists;
using OXDesk.Core.Settings;
using OXDesk.Core.Workflows;

namespace OXDesk.DbContext.Data
{
    /// <summary>
    /// Database context dedicated to tenant-scoped entities.
    /// </summary>
    public class TenantDbContext : IdentityDbContext<ApplicationUser, ApplicationRole, int, ApplicationUserClaim, ApplicationUserRole, ApplicationUserLogin, ApplicationRoleClaim, ApplicationUserToken>
    {
        private readonly ICurrentTenant _currentTenant;
        private readonly string _hostingModel;

        /// <summary>
        /// Initializes a new instance of the <see cref="TenantDbContext"/> class.
        /// </summary>
        /// <param name="options">Typed options specific to the tenant context.</param>
        /// <param name="currentTenant">Provides the tenant schema for multi-schema support.</param>
        /// <param name="configuration">Application configuration.</param>
        [ActivatorUtilitiesConstructor]
        public TenantDbContext(
            DbContextOptions<TenantDbContext> options,
            ICurrentTenant currentTenant,
            IConfiguration configuration)
            : base(options)
        {
            _currentTenant = currentTenant;
            _hostingModel = configuration["HostingModel"] ?? CommonConstant.HOSTING_MODEL_OS;
        }

        /// <summary>
        /// Configures the database connection with schema support.
        /// </summary>
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            base.OnConfiguring(optionsBuilder);

            var tenantSchema = _currentTenant?.Schema;
            if (_hostingModel == CommonConstant.HOSTING_MODEL_CLOUD && !string.IsNullOrEmpty(tenantSchema))
            {
                optionsBuilder.UseNpgsql(o => o.MigrationsHistoryTable("__EFMigrationsHistory", tenantSchema));
            }
        }

        /// <summary>
        /// Gets or sets tenant-scoped DbSets.
        /// </summary>
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
        public DbSet<FormType> FormTypes { get; set; } = null!;
        public DbSet<Status> Statuses { get; set; } = null!;
        public DbSet<Channel> Channels { get; set; } = null!;
        public DbSet<Brand> Brands { get; set; } = null!;
        public DbSet<Priority> Priorities { get; set; } = null!;
        public DbSet<Workflow> Workflows { get; set; } = null!;
        public DbSet<WorkFlowStatus> WorkFlowStatuses { get; set; } = null!;
        public DbSet<WorkflowMapping> WorkflowMappings { get; set; } = null!;
        public DbSet<UserSignature> UserSignatures { get; set; } = null!;
        public DbSet<BusinessHours> BusinessHours { get; set; } = null!;
        public DbSet<CustomBusinessHours> CustomBusinessHours { get; set; } = null!;
        public DbSet<Holiday> Holidays { get; set; } = null!;
        public DbSet<Setting> Settings { get; set; } = null!;

        /// <summary>
        /// Configures tenant-scoped global query filters.
        /// </summary>
        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Configure OpenIddict to use default entity types
            builder.UseOpenIddict();

            // Exclude Tenant configuration (that's in ApplicationDbContext)
            builder.ApplyConfigurationsFromAssembly(
                Assembly.Load("OXDesk.Infrastructure"),
                t => t.Namespace != null && !t.Namespace.Contains(".Data.Tenants"));

            // Determine schema based on hosting model
            var tenantSchema = _currentTenant?.Schema;
            string schema = "public";
            
            if (_hostingModel == CommonConstant.HOSTING_MODEL_CLOUD && string.IsNullOrEmpty(tenantSchema))
            {
                throw new Exception("Tenant schema is not set");
            }

            if (!string.IsNullOrEmpty(tenantSchema))
            {
                schema = tenantSchema;
            }

            // Apply snake_case naming convention to all tables, columns, keys and indexes
            foreach (var entity in builder.Model.GetEntityTypes())
            {
                // table names – default snake_case
                var defaultTableName = entity.GetTableName()!.ToSnakeCase();

                // For ASP.NET Identity tables, override to concise names (users, roles, etc.)
                defaultTableName = entity.ClrType switch
                {
                    Type t when t == typeof(ApplicationUser)            => "users",
                    Type t when t == typeof(ApplicationRole)            => "roles",
                    Type t when t == typeof(ApplicationUserRole)        => "user_roles",
                    Type t when t == typeof(ApplicationUserClaim)       => "user_claims",
                    Type t when t == typeof(ApplicationUserLogin)       => "user_logins",
                    Type t when t == typeof(ApplicationRoleClaim)       => "role_claims",
                    Type t when t == typeof(ApplicationUserToken)       => "user_tokens",
                    _                                                   => defaultTableName
                };

                entity.SetTableName(defaultTableName);
                entity.SetSchema(schema);

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
        /// Saves changes to the database and automatically sets TenantId for tenant-scoped entities.
        /// </summary>
        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            var tenantId = _currentTenant?.Id;
            
            // Automatically set TenantId for entities that have it
            foreach (var entry in ChangeTracker.Entries())
            {
                if (entry.State == EntityState.Added || entry.State == EntityState.Modified)
                {
                    var tenantIdProperty = entry.Entity.GetType().GetProperty("TenantId");
                    if (tenantIdProperty != null && tenantIdProperty.PropertyType == typeof(Guid?))
                    {
                        var currentValue = tenantIdProperty.GetValue(entry.Entity) as Guid?;
                        if (entry.State == EntityState.Added && currentValue == null)
                        {
                            tenantIdProperty.SetValue(entry.Entity, tenantId);
                        }
                    }
                }
            }

            return await base.SaveChangesAsync(cancellationToken);
        }

        /// <summary>
        /// Saves changes to the database and automatically sets TenantId for tenant-scoped entities.
        /// </summary>
        public override int SaveChanges()
        {
            var tenantId = _currentTenant?.Id;
            
            // Automatically set TenantId for entities that have it
            foreach (var entry in ChangeTracker.Entries())
            {
                if (entry.State == EntityState.Added || entry.State == EntityState.Modified)
                {
                    var tenantIdProperty = entry.Entity.GetType().GetProperty("TenantId");
                    if (tenantIdProperty != null && tenantIdProperty.PropertyType == typeof(Guid?))
                    {
                        var currentValue = tenantIdProperty.GetValue(entry.Entity) as Guid?;
                        if (entry.State == EntityState.Added && currentValue == null)
                        {
                            tenantIdProperty.SetValue(entry.Entity, tenantId);
                        }
                    }
                }
            }

            return base.SaveChanges();
        }
    }
}
