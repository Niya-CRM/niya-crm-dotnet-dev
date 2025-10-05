using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OXDesk.Core.AuditLogs.ChangeHistory;

namespace OXDesk.Infrastructure.Data.AuditLogs.ChangeHistory
{
    /// <summary>
    /// Entity Framework configuration for the ChangeHistoryLog entity.
    /// Configures database table and column mappings.
    /// </summary>
    public class ChangeHistoryLogConfiguration : IEntityTypeConfiguration<ChangeHistoryLog>
    {
        public void Configure(EntityTypeBuilder<ChangeHistoryLog> builder)
        {            
            builder.HasKey(o => o.Id);
            builder.Property(o => o.Id)
                   .UseIdentityByDefaultColumn()
                   .HasIdentityOptions(startValue: 1000001L);

            // Index for tenant_id for efficient multi-tenant filtering
            builder.HasIndex(c => new { c.TenantId, c.CreatedAt });

            // Composite indexes for efficient filtering by entity (separate for UUID and int)
            builder.HasIndex(c => new { c.TenantId, c.ObjectKey, c.ObjectItemIdUuid, c.CreatedAt });
            builder.HasIndex(c => new { c.TenantId, c.ObjectKey, c.ObjectItemIdInt, c.CreatedAt });
            builder.HasIndex(c => new { c.TenantId, c.ObjectKey, c.CreatedAt });
        }
    }
}
