using System.Reflection;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using NiyaCRM.Core.Identity;
using Microsoft.EntityFrameworkCore;
using NiyaCRM.Core.Helpers.Naming;
using NiyaCRM.Core.AuditLogs;
using NiyaCRM.Core.AuditLogs.ChangeHistory;
using NiyaCRM.Core.Tenants;
using NiyaCRM.Core.Referentials;
using NiyaCRM.Core.DynamicObjects;
using NiyaCRM.Core.DynamicObjects.Fields;
using NiyaCRM.Core.ValueLists;
using NiyaCRM.Core.AppInstallation;

namespace NiyaCRM.Infrastructure.Data
{
    /// <summary>
    /// Application database context.
    /// </summary>
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser, ApplicationRole, Guid, IdentityUserClaim<Guid>, ApplicationUserRole, IdentityUserLogin<Guid>, ApplicationRoleClaim, IdentityUserToken<Guid>>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ApplicationDbContext" /> class.
        /// </summary>
        /// <param name="options">The options for the database context.</param>
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        /// <summary>
        /// Gets or sets the DbSet.
        /// </summary>
        public DbSet<Tenant> Tenants { get; set; } = null!;
        public DbSet<AuditLog> AuditLogs { get; set; } = null!;
        public DbSet<Country> Countries { get; set; } = null!;
        public DbSet<DynamicObject> DynamicObjects { get; set; } = null!;
        public DbSet<ValueList> ValueLists { get; set; } = null!;
        public DbSet<ValueListItem> ValueListItems { get; set; } = null!;
        public DbSet<ChangeHistoryLog> ChangeHistoryLogs { get; set; } = null!;
        public DbSet<AppInstallationStatus> AppInstallationStatus { get; set; } = null!;
        public DbSet<Permission> Permissions { get; set; } = null!;
        public DbSet<DynamicObjectField> DynamicObjectFields { get; set; } = null!;
        public DbSet<DynamicObjectFieldType> DynamicObjectFieldTypes { get; set; } = null!;
        public DbSet<RefreshToken> RefreshTokens { get; set; } = null!;

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
                    Type t when t == typeof(ApplicationRole) || t == typeof(IdentityRole<Guid>)                 => "roles",
                    Type t when t == typeof(IdentityUserRole<Guid>)     => "user_roles",
                    Type t when t == typeof(IdentityUserClaim<Guid>)    => "user_claims",
                    Type t when t == typeof(IdentityUserLogin<Guid>)    => "user_logins",
                    Type t when t == typeof(IdentityRoleClaim<Guid>)    => "role_claims",
                    Type t when t == typeof(IdentityUserToken<Guid>)    => "user_tokens",
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
        }
    }
}
