using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NiyaCRM.Core.AuditLogs;

namespace NiyaCRM.Infrastructure.Data.AuditLogs
{
    /// <summary>
    /// Entity Framework configuration for the AuditLog entity.
    /// Configures database table and column mappings.
    /// </summary>
    public class AuditLogEntity : IEntityTypeConfiguration<AuditLog>
    {
        public void Configure(EntityTypeBuilder<AuditLog> builder)
        {
            // Table name
            builder.ToTable("audit_logs");

            // Primary key
            builder.HasKey(a => a.Id);
            builder.Property(a => a.Id)
                .HasColumnName("id")
                .IsRequired();

            builder.Property(a => a.Module)
                .HasColumnName("module")
                .HasMaxLength(100)
                .IsRequired();

            builder.Property(a => a.Event)
                .HasColumnName("event")
                .HasMaxLength(100)
                .IsRequired();

            builder.Property(a => a.MappedId)
                .HasColumnName("mapped_id")
                .HasMaxLength(100)
                .IsRequired();

            builder.Property(a => a.IP)
                .HasColumnName("ip")
                .HasMaxLength(45)
                .IsRequired(false);

            builder.Property(a => a.Data)
                .HasColumnName("data")
                .HasMaxLength(4000)
                .IsRequired();

            builder.Property(a => a.CreatedAt)
                .HasColumnName("created_at")
                .IsRequired();

            builder.Property(a => a.CreatedBy)
                .HasColumnName("created_by")
                .HasMaxLength(256)
                .IsRequired();
            
            // Composite index for efficient filtering
            builder.HasIndex(a => new { a.Module, a.MappedId, a.CreatedAt });
        }
    }
}
