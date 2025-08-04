using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NiyaCRM.Core.AuditLogs.ChangeHistory;

namespace NiyaCRM.Infrastructure.Data.AuditLogs.ChangeHistory
{
    /// <summary>
    /// Entity Framework configuration for the ChangeHistoryLog entity.
    /// Configures database table and column mappings.
    /// </summary>
    public class ChangeHistoryLogConfiguration : IEntityTypeConfiguration<ChangeHistoryLog>
    {
        public void Configure(EntityTypeBuilder<ChangeHistoryLog> builder)
        {            
            // Composite index for efficient filtering by entity
            builder.HasIndex(c => new { c.ObjectKey, c.ObjectItemId, c.CreatedAt });
            
            // Index for filtering by field name
            builder.HasIndex(c => new { c.ObjectKey, c.ObjectItemId, c.FieldName, c.CreatedAt });
            
            // Index for time-based queries
            builder.HasIndex(c => c.CreatedAt);
        }
    }
}
