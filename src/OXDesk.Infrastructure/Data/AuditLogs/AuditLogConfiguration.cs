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

            builder.HasIndex(a => new { a.ObjectKey, a.ObjectItemIdUuid, a.ObjectItemIdInt })
                   .HasDatabaseName("ix_audit_logs_object_keys");

            builder.HasIndex(a => a.CreatedAt)
                   .HasDatabaseName("ix_audit_logs_created_at");
        }
    }
}
