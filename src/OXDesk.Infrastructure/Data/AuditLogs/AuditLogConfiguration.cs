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

            // Index for tenant_id for efficient multi-tenant filtering
            builder.HasIndex(a => new { a.TenantId, a.CreatedAt });

            // Composite indexes for efficient filtering (separate for UUID and int)
            builder.HasIndex(a => new { a.TenantId, a.ObjectKey, a.ObjectItemIdUuid, a.CreatedAt });
            builder.HasIndex(a => new { a.TenantId, a.ObjectKey, a.ObjectItemIdInt, a.CreatedAt });
            builder.HasIndex(a => new { a.TenantId, a.ObjectKey, a.CreatedAt });
        }
    }
}
