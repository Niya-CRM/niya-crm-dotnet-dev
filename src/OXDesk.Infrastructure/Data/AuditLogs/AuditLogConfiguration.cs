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
                   .HasIdentityOptions(startValue: 10001L);

            builder.HasIndex(a => new { a.ObjectId, a.ObjectItemIdUuid, a.ObjectItemIdInt })
                   .HasDatabaseName("ix_audit_logs_object_id_object_item_ids");

            builder.HasIndex(a => new { a.ObjectId, a.CreatedAt })
                   .HasDatabaseName("ix_audit_logs_object_id_created_at");
        }
    }
}
