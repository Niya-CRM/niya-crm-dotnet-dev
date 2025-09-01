using System;
using System.Reflection;
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
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser, ApplicationRole, Guid, ApplicationUserClaim, ApplicationUserRole, ApplicationUserLogin, ApplicationRoleClaim, ApplicationUserToken>
    {
        private readonly IServiceProvider _serviceProvider;

        /// <summary>
        /// Initializes a new instance of the <see cref="ApplicationDbContext" /> class.
        /// </summary>
        /// <param name="options">The options for the database context.</param>
        /// <param name="serviceProvider">The service provider to resolve services.</param>
        public ApplicationDbContext(
            DbContextOptions<ApplicationDbContext> options,
            IServiceProvider serviceProvider)
            : base(options)
        {
            _serviceProvider = serviceProvider;
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
        public DbSet<RefreshToken> RefreshTokens { get; set; } = null!;
        public DbSet<Ticket> Tickets { get; set; } = null!;
        public DbSet<TicketStatus> TicketStatuses { get; set; } = null!;
        public DbSet<Channel> Channels { get; set; } = null!;
        public DbSet<Brand> Brands { get; set; } = null!;
        public DbSet<Priority> Priorities { get; set; } = null!;

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
        }
        
        /// <summary>
        /// Sets the PostgreSQL session variable app.tenant_id if a tenant_id is available from the tenant provider
        /// This method is kept for backward compatibility but is deprecated
        /// </summary>
        [Obsolete("This method is deprecated. Use parameterized queries with tenant ID instead.")]
        public void SetTenantIdSessionVariable()
        {
            try
            {
                // Only try to get the tenant provider if we're in a scope
                if (_serviceProvider == null)
                {
                    return;
                }
                
                // Try to get the tenant provider from the current scope
                // This will only work during a request, not during startup
                var httpContextAccessor = _serviceProvider.GetService(typeof(Microsoft.AspNetCore.Http.IHttpContextAccessor)) as Microsoft.AspNetCore.Http.IHttpContextAccessor;
                if (httpContextAccessor?.HttpContext == null)
                {
                    return; // No HTTP context available
                }
                
                // Get tenant ID from HttpContext.Items directly
                if (httpContextAccessor.HttpContext.Items.TryGetValue("tenant_id", out var tenantIdObj) && tenantIdObj is Guid tenantId)
                {
                    // Set the PostgreSQL session variable using a parameterized query to prevent SQL injection
                    var tenantIdParam = new NpgsqlParameter("tenantId", tenantId);
                    this.Database.ExecuteSqlRaw("SET LOCAL \"app.tenant_id\" = @tenantId;", tenantIdParam);
                }
            }
            catch (Exception ex)
            {
                // Log the error but don't stop execution
                // In production, you would want to use a proper logging mechanism here
                System.Diagnostics.Debug.WriteLine($"Error setting tenant_id session variable: {ex.Message}");
            }
        }
        
        /// <summary>
        /// Creates a SQL parameter for tenant ID to be used in parameterized queries
        /// </summary>
        /// <returns>NpgsqlParameter with the current tenant ID</returns>
        public NpgsqlParameter CreateTenantIdParameter()
        {
            var tenantId = GetCurrentTenantId();
            return new NpgsqlParameter("tenantId", tenantId);
        }
        
        /// <summary>
        /// Sets the current tenant ID as a PostgreSQL session variable
        /// </summary>
        public void SetCurrentTenantIdSessionVariable()
        {
            try
            {
                var tenantId = GetCurrentTenantId();
                var tenantIdParam = new NpgsqlParameter("tenantId", tenantId);
                this.Database.ExecuteSqlRaw("SET LOCAL \"app.current_tenant_id\" = @tenantId;", tenantIdParam);
            }
            catch (Exception ex)
            {
                // Log the error but don't stop execution
                System.Diagnostics.Debug.WriteLine($"Error setting app.current_tenant_id session variable: {ex.Message}");
            }
        }
        
        /// <summary>
        /// Gets the current tenant ID from the HTTP context
        /// </summary>
        /// <returns>The current tenant ID</returns>
        /// <exception cref="InvalidOperationException">Thrown when tenant ID is not found</exception>
        public Guid GetCurrentTenantId()
        {
            // Only try to get the tenant provider if we're in a scope
            if (_serviceProvider == null)
            {
                throw new InvalidOperationException("Service provider is not available");
            }
            
            // Try to get the tenant provider from the current scope
            var httpContextAccessor = _serviceProvider.GetService(typeof(Microsoft.AspNetCore.Http.IHttpContextAccessor)) as Microsoft.AspNetCore.Http.IHttpContextAccessor;
            if (httpContextAccessor?.HttpContext == null)
            {
                throw new InvalidOperationException("HTTP context is not available");
            }
            
            // Get tenant ID from HttpContext.Items directly
            if (httpContextAccessor.HttpContext.Items.TryGetValue("tenant_id", out var tenantIdObj) && tenantIdObj is Guid tenantId)
            {
                return tenantId;
            }
            
            throw new InvalidOperationException("Tenant ID not found in HTTP context");
        }
    }
}
