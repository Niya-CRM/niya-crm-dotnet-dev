using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OXDesk.Core.AuditLogs;

namespace OXDesk.Infrastructure.Data.AuditLogs
{
    /// <summary>
    /// Entity Framework configuration for the AuditLog entity.
    /// Configures database table and column mappings.
    /// </summary>
    public class AuditLogConfiguration : IEntityTypeConfiguration<AuditLog>
    {
        public void Configure(EntityTypeBuilder<AuditLog> builder)
        {            
            builder.HasKey(o => o.Id);
            builder.Property(o => o.Id)
                   .UseIdentityByDefaultColumn()
                   .HasIdentityOptions(startValue: 1000001L);

            // Index for tenant_id for efficient multi-tenant filtering
            builder.HasIndex(a => new { a.TenantId, a.CreatedAt });

            // Composite index for efficient filtering
            builder.HasIndex(a => new { a.TenantId, a.ObjectKey, a.ObjectItemId, a.CreatedAt });
            builder.HasIndex(a => new { a.TenantId, a.ObjectKey, a.CreatedAt });
        }
    }
}
