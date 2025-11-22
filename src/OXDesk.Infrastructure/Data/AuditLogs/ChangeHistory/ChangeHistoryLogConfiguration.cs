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
                   .HasIdentityOptions(startValue: 10001L);

            builder.HasIndex(c => new { c.ObjectId, c.ObjectItemIdInt })
                   .HasDatabaseName("ix_change_history_logs_object_id_object_item_id");
        }
    }
}
