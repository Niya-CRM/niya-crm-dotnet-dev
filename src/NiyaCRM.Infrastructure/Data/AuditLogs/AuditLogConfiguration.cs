using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NiyaCRM.Core.AuditLogs;

namespace NiyaCRM.Infrastructure.Data.AuditLogs
{
    /// <summary>
    /// Entity Framework configuration for the AuditLog entity.
    /// Configures database table and column mappings.
    /// </summary>
    public class AuditLogConfiguration : IEntityTypeConfiguration<AuditLog>
    {
        public void Configure(EntityTypeBuilder<AuditLog> builder)
        {            
            // Composite index for efficient filtering
            builder.HasIndex(a => new { a.Module, a.MappedId, a.CreatedAt });
            builder.HasIndex(a => new { a.Module, a.CreatedAt });
        }
    }
}
